using System.Diagnostics;
using HALF.Watch;

namespace HALF.Host.Commands;

internal sealed class RunCommand(HalfHostConfiguration configuration, IRunRecordRepository runRecordRepository) : IHostCommand
{
    public CommandDescriptor Descriptor => new("run", "Execute a local model run through the host runtime.");

    public CommandExecutionResult Execute(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid();

        var isStreaming = args.Any(static argument =>
            string.Equals(argument, "--stream", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(argument, "--streaming", StringComparison.OrdinalIgnoreCase));

        var shouldFail = args.Any(static argument =>
            string.Equals(argument, "--fail", StringComparison.OrdinalIgnoreCase));

        var prompt = string.Join(' ', args.Where(argument => !argument.StartsWith("--", StringComparison.Ordinal)));
        var promptTokenCount = Math.Max(1, prompt.Length / 4);
        var completionTokenCount = shouldFail ? 0 : 48;

        var loadLatencyMs = 25;
        var promptLatencyMs = 15;
        var generationLatencyMs = shouldFail ? 0 : 60;

        stopwatch.Stop();

        var totalLatencyMs = Math.Max(
            loadLatencyMs + promptLatencyMs + generationLatencyMs,
            (int)Math.Max(1L, stopwatch.ElapsedMilliseconds));

        var runRecord = new RunRecord(
            new RunIdentity(requestId),
            new RunModel(
                configuration.Runtime.RuntimeName,
                configuration.Runtime.ModelName,
                configuration.Runtime.Quantization),
            new RunTiming(
                totalLatencyMs,
                loadLatencyMs,
                promptLatencyMs,
                generationLatencyMs),
            new RunTokens(promptTokenCount, completionTokenCount),
            isStreaming,
            new RunOutcome(!shouldFail, shouldFail ? "runtime_error" : null));

        runRecordRepository.InsertCompleted(runRecord);

        return shouldFail
            ? new CommandExecutionResult(1, $"Run failed and metadata persisted (request_id={requestId:D}).")
            : new CommandExecutionResult(0, $"Run completed and metadata persisted (request_id={requestId:D}).");
    }
}