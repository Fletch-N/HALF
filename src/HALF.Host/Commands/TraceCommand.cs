using HALF.Watch;

namespace HALF.Host.Commands;

internal sealed class TraceCommand(IRunRecordRepository runRecordRepository) : IHostCommand
{
    public CommandDescriptor Descriptor => new("trace", "Inspect or export recorded traces for prior executions.");

    public CommandExecutionResult Execute(string[] args)
    {
        if (TryGetRequestId(args, out var requestId))
        {
            var record = runRecordRepository.GetByRequestId(requestId);

            return record is null
                ? new CommandExecutionResult(1, $"No persisted run found for request_id={requestId:D}.")
                : new CommandExecutionResult(0, FormatRunRecord(record));
        }

        var limit = TryGetLimit(args, out var parsedLimit) ? parsedLimit : 10;
        var records = runRecordRepository.ListRecent(limit);

        if (records.Count == 0)
        {
            return new CommandExecutionResult(0, "No persisted runs found.");
        }

        var lines = records
            .Select(static record =>
                $"{record.Identity.ExecutedAtUtc:O} request_id={record.Identity.RequestId:D} model={record.Model.ModelName} success={record.Outcome.IsSuccess} total_latency_ms={record.Timing.TotalLatencyMs}");

        return new CommandExecutionResult(0, string.Join(Environment.NewLine, lines));
    }

    private static string FormatRunRecord(RunRecord record)
    {
        return string.Join(
            Environment.NewLine,
            [
                $"executed_at_utc={record.Identity.ExecutedAtUtc:O}",
                $"request_id={record.Identity.RequestId:D}",
                $"runtime_name={record.Model.RuntimeName}",
                $"model_name={record.Model.ModelName}",
                $"quantization={record.Model.Quantization ?? ""}",
                $"total_latency_ms={record.Timing.TotalLatencyMs}",
                $"load_latency_ms={record.Timing.LoadLatencyMs}",
                $"prompt_latency_ms={record.Timing.PromptLatencyMs}",
                $"generation_latency_ms={record.Timing.GenerationLatencyMs}",
                $"prompt_tokens={record.Tokens.PromptTokens}",
                $"completion_tokens={record.Tokens.CompletionTokens}",
                $"is_streaming={record.IsStreaming}",
                $"is_success={record.Outcome.IsSuccess}",
                $"error_category={record.Outcome.ErrorCategory ?? ""}"
            ]);
    }

    private static bool TryGetRequestId(string[] args, out Guid requestId)
    {
        requestId = Guid.Empty;

        for (var index = 0; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--request-id", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (index + 1 >= args.Length)
            {
                return false;
            }

            return Guid.TryParse(args[index + 1], out requestId);
        }

        return false;
    }

    private static bool TryGetLimit(string[] args, out int limit)
    {
        limit = 10;

        for (var index = 0; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--limit", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (index + 1 >= args.Length)
            {
                return false;
            }

            return int.TryParse(args[index + 1], out limit);
        }

        return false;
    }
}