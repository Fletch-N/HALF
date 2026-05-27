namespace HALF.Watch.Tests;

public sealed class SqliteRunRecordRepositoryTests
{
    [Fact]
    public void InsertCompleted_PersistsAndCanFetchByRequestId()
    {
        var sqlitePath = CreateSqlitePath();
        var repository = new SqliteRunRecordRepository(sqlitePath);
        repository.EnsureInitialized();

        var requestId = Guid.NewGuid();
        var record = new RunRecord(
            new RunIdentity(requestId),
            new RunModel("ollama", "qwen3.5:4b", "q4_k_m"),
            new RunTiming(120, 15, 20, 85),
            new RunTokens(24, 32),
            false,
            new RunOutcome(true, null));

        repository.InsertCompleted(record);

        var persisted = repository.GetByRequestId(requestId);

        Assert.NotNull(persisted);
        Assert.Equal(record, persisted!);
    }

    [Fact]
    public void ListRecent_AndSummary_ReflectPersistedRows()
    {
        var sqlitePath = CreateSqlitePath();
        var repository = new SqliteRunRecordRepository(sqlitePath);
        repository.EnsureInitialized();

        var successfulRecord = new RunRecord(
            new RunIdentity(Guid.NewGuid()),
            new RunModel("ollama", "qwen3.5:4b", null),
            new RunTiming(100, 10, 20, 70),
            new RunTokens(10, 20),
            false,
            new RunOutcome(true, null));

        var failedRecord = new RunRecord(
            new RunIdentity(Guid.NewGuid()),
            new RunModel("ollama", "qwen3.5:4b", null),
            new RunTiming(200, 30, 50, 120),
            new RunTokens(14, 0),
            true,
            new RunOutcome(false, "runtime_error"));

        repository.InsertCompleted(successfulRecord);
        repository.InsertCompleted(failedRecord);

        var recent = repository.ListRecent(5);
        var summary = repository.GetSummary();

        Assert.Equal(2, recent.Count);
        Assert.Equal(2, summary.TotalRuns);
        Assert.Equal(1, summary.SuccessfulRuns);
        Assert.Equal(1, summary.FailedRuns);
        Assert.True(summary.AverageTotalLatencyMs > 0d);
        Assert.NotNull(summary.MostRecent);
    }

    [Fact]
    public void Records_AreDurableAcrossRepositoryInstances()
    {
        var sqlitePath = CreateSqlitePath();
        var requestId = Guid.NewGuid();

        var writer = new SqliteRunRecordRepository(sqlitePath);
        writer.EnsureInitialized();
        writer.InsertCompleted(
            new RunRecord(
                new RunIdentity(requestId),
                new RunModel("ollama", "phi4-mini:3.8b", null),
                new RunTiming(90, 10, 20, 60),
                new RunTokens(12, 24),
                false,
                new RunOutcome(true, null)));

        var reader = new SqliteRunRecordRepository(sqlitePath);
        var record = reader.GetByRequestId(requestId);

        Assert.NotNull(record);
        Assert.Equal(requestId, record!.Identity.RequestId);
    }

    private static string CreateSqlitePath()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), "HALF", "watch-tests", Guid.NewGuid().ToString("N"));
        return Path.Combine(testRoot, "half.sqlite");
    }
}