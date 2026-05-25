namespace HALF.Watch;

public sealed record RunRecord(
    RunIdentity Identity,
    RunModel Model,
    RunTiming Timing,
    RunTokens Tokens,
    bool IsStreaming,
    RunOutcome Outcome);

public sealed record RunIdentity(Guid RequestId);
public sealed record RunModel(string RuntimeName, string ModelName, string? Quantization);
public sealed record RunTiming(int TotalLatencyMs, int LoadLatencyMs, int PromptLatencyMs, int GenerationLatencyMs);
public sealed record RunTokens(int PromptTokens, int CompletionTokens);
public sealed record RunOutcome(bool IsSuccess, string? ErrorCategory);


public static class RunRecordSchema
{
    public static IReadOnlyDictionary<string, string> FieldMap { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["request_id"] = "Identity.RequestId",
            ["runtime_name"] = "Model.RuntimeName",
            ["model_name"] = "Model.ModelName",
            ["quantization"] = "Model.Quantization",
            ["total_latency_ms"] = "Timing.TotalLatencyMs",
            ["load_latency_ms"] = "Timing.LoadLatencyMs",
            ["prompt_latency_ms"] = "Timing.PromptLatencyMs",
            ["generation_latency_ms"] = "Timing.GenerationLatencyMs",
            ["prompt_tokens"] = "Tokens.PromptTokens",
            ["completion_tokens"] = "Tokens.CompletionTokens",
            ["is_streaming"] = "IsStreaming",
            ["is_success"] = "Outcome.IsSuccess",
            ["error_category"] = "Outcome.ErrorCategory"
        };
}