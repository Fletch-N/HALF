using HALF.Watch;

namespace HALF.Host.Commands;

internal sealed class StatusCommand(HalfHostConfiguration configuration, IRunRecordRepository runRecordRepository) : IHostCommand
{
    public CommandDescriptor Descriptor => new("status", "Report the current runtime and observability status.");

    public CommandExecutionResult Execute(string[] args)
    {
        var summary = runRecordRepository.GetSummary();

        var lines = new List<string>
        {
            "HALF Host Status",
            $"runtime_endpoint={configuration.Runtime.Endpoint}",
            $"runtime_name={configuration.Runtime.RuntimeName}",
            $"model_name={configuration.Runtime.ModelName}",
            $"quantization={configuration.Runtime.Quantization ?? ""}",
            $"sqlite_path={configuration.Storage.SqlitePath}",
            $"runs_total={summary.TotalRuns}",
            $"runs_successful={summary.SuccessfulRuns}",
            $"runs_failed={summary.FailedRuns}",
            $"avg_total_latency_ms={summary.AverageTotalLatencyMs:F2}"
        };

        if (summary.MostRecent is not null)
        {
            lines.Add($"most_recent_request_id={summary.MostRecent.Identity.RequestId:D}");
            lines.Add($"most_recent_executed_at_utc={summary.MostRecent.Identity.ExecutedAtUtc:O}");
        }

        return new CommandExecutionResult(0, string.Join(Environment.NewLine, lines));
    }
}