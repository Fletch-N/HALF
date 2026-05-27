using Microsoft.Data.Sqlite;

namespace HALF.Watch;

public interface IRunRecordRepository
{
    void EnsureInitialized();
    void InsertCompleted(RunRecord record);
    RunRecord? GetByRequestId(Guid requestId);
    IReadOnlyList<RunRecord> ListRecent(int limit);
    RunRecordRepositorySummary GetSummary();

}

public sealed record RunRecordRepositorySummary(
    int TotalRuns,
    int SuccessfulRuns,
    int FailedRuns,
    double AverageTotalLatencyMs,
    RunRecord? MostRecent);

public sealed class SqliteRunRecordRepository(string sqlitePath) : IRunRecordRepository
{
    private readonly string path = Path.GetFullPath(sqlitePath);

    public void EnsureInitialized()
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = OpenConnection(path);
        using var command = connection.CreateCommand();
        command.CommandText = RunRecordSql.CreateTable;
        _ = command.ExecuteNonQuery();
    }

    public void InsertCompleted(RunRecord record)
    {
        using var connection = OpenConnection(path);
        using var command = connection.CreateCommand();
        command.CommandText = RunRecordSql.InsertCompleted;
        BindParameters(command, record);
        _ = command.ExecuteNonQuery();
    }

    private static void BindParameters(SqliteCommand command, RunRecord record)
    {
        var bind = command.Parameters.AddWithValue;

        bind("$request_id", record.Identity.RequestId.ToString("D"));
        bind("$executed_at_utc", record.Identity.ExecutedAtUtc.ToString("O"));
        bind("$runtime_name", record.Model.RuntimeName);
        bind("$model_name", record.Model.ModelName);
        bind("$quantization", (object?)record.Model.Quantization ?? DBNull.Value);
        bind("$total_latency_ms", record.Timing.TotalLatencyMs);
        bind("$load_latency_ms", record.Timing.LoadLatencyMs);
        bind("$prompt_latency_ms", record.Timing.PromptLatencyMs);
        bind("$generation_latency_ms", record.Timing.GenerationLatencyMs);
        bind("$prompt_tokens", record.Tokens.PromptTokens);
        bind("$completion_tokens", record.Tokens.CompletionTokens);
        bind("$is_streaming", record.IsStreaming ? 1 : 0);
        bind("$is_success", record.Outcome.IsSuccess ? 1 : 0);
        bind("$error_category", (object?)record.Outcome.ErrorCategory ?? DBNull.Value);
    }

    public RunRecord? GetByRequestId(Guid requestId)
    {
        using var connection = OpenConnection(path);
        using var command = connection.CreateCommand();

        command.CommandText = RunRecordSql.SelectByRequestId;

        command.Parameters.AddWithValue("$request_id", requestId.ToString("D"));

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return MapRecord(reader);
    }

    public IReadOnlyList<RunRecord> ListRecent(int limit)
    {
        var safeLimit = Math.Max(1, limit);
        var results = new List<RunRecord>(safeLimit);

        using var connection = OpenConnection(path);
        using var command = connection.CreateCommand();

        command.CommandText = RunRecordSql.SelectRecent;

        command.Parameters.AddWithValue("$limit", safeLimit);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            results.Add(MapRecord(reader));
        }

        return results;
    }

    public RunRecordRepositorySummary GetSummary()
    {
        using var connection = OpenConnection(path);
        using var command = connection.CreateCommand();
        command.CommandText = RunRecordSql.SelectSummary;

        using var reader = command.ExecuteReader();
        reader.Read();

        var totalRuns = reader.GetInt32(0);
        var successfulRuns = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
        var failedRuns = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
        var averageTotalLatency = reader.IsDBNull(3) ? 0d : reader.GetDouble(3);

        using var mostRecentCommand = connection.CreateCommand();
        mostRecentCommand.CommandText = RunRecordSql.SelectRecent;
        mostRecentCommand.Parameters.AddWithValue("$limit", 1);

        using var recentReader = mostRecentCommand.ExecuteReader();

        var mostRecent = recentReader.Read()
            ? MapRecord(recentReader)
            : null;

        return new RunRecordRepositorySummary(
            totalRuns,
            successfulRuns,
            failedRuns,
            averageTotalLatency,
            mostRecent);
    }

    private static SqliteConnection OpenConnection(string fullPath)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString());

        connection.Open();

        return connection;
    }

    private static RunRecord MapRecord(SqliteDataReader reader)
    {
        var requestId = Guid.Parse(reader.GetString(0));
        var executedAtUtc = DateTimeOffset.Parse(reader.GetString(1));
        var runtimeName = reader.GetString(2);
        var modelName = reader.GetString(3);
        var quantization = reader.IsDBNull(4) ? null : reader.GetString(4);
        var totalLatencyMs = reader.GetInt32(5);
        var loadLatencyMs = reader.GetInt32(6);
        var promptLatencyMs = reader.GetInt32(7);
        var generationLatencyMs = reader.GetInt32(8);
        var promptTokens = reader.GetInt32(9);
        var completionTokens = reader.GetInt32(10);
        var isStreaming = reader.GetInt32(11) == 1;
        var isSuccess = reader.GetInt32(12) == 1;
        var errorCategory = reader.IsDBNull(13) ? null : reader.GetString(13);

        return new RunRecord(
            new RunIdentity(requestId, executedAtUtc),
            new RunModel(runtimeName, modelName, quantization),
            new RunTiming(totalLatencyMs, loadLatencyMs, promptLatencyMs, generationLatencyMs),
            new RunTokens(promptTokens, completionTokens),
            isStreaming,
            new RunOutcome(isSuccess, errorCategory));
    }
}
