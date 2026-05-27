using HALF.Watch;

namespace HALF.Watch.Tests;

public sealed class RunRecordTests
{
    [Fact]
    public void RunRecord_CanRepresentSuccessfulExecution()
    {
        var requestId = Guid.NewGuid();
        var record = new RunRecord(
            new RunIdentity(requestId),
            new RunModel("ollama", "qwen3.5:4b", "Q4_K_M"),
            new RunTiming(7532, 297, 137, 6885),
            new RunTokens(102, 158),
            false,
            new RunOutcome(true, null));

        Assert.Equal(requestId, record.Identity.RequestId);
        Assert.Equal("ollama", record.Model.RuntimeName);
        Assert.Equal("qwen3.5:4b", record.Model.ModelName);
        Assert.Equal("Q4_K_M", record.Model.Quantization);
        Assert.Equal(7532, record.Timing.TotalLatencyMs);
        Assert.Equal(297, record.Timing.LoadLatencyMs);
        Assert.Equal(137, record.Timing.PromptLatencyMs);
        Assert.Equal(6885, record.Timing.GenerationLatencyMs);
        Assert.Equal(102, record.Tokens.PromptTokens);
        Assert.Equal(158, record.Tokens.CompletionTokens);
        Assert.False(record.IsStreaming);
        Assert.True(record.Outcome.IsSuccess);
        Assert.Null(record.Outcome.ErrorCategory);
    }

    [Fact]
    public void RunRecord_CanRepresentFailedExecution()
    {
        var record = new RunRecord(
            new RunIdentity(Guid.NewGuid()),
            new RunModel("ollama", "nemotron-3-nano:4b", null),
            new RunTiming(250, 25, 75, 150),
            new RunTokens(42, 0),
            true,
            new RunOutcome(false, "runtime_error"));

        Assert.True(record.IsStreaming);
        Assert.False(record.Outcome.IsSuccess);
        Assert.Equal("runtime_error", record.Outcome.ErrorCategory);
        Assert.Null(record.Model.Quantization);
    }

    [Fact]
    public void FieldMap_RepresentsEveryRequiredTelemetryField()
    {
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["request_id"] = "Identity.RequestId",
            ["executed_at_utc"] = "Identity.ExecutedAtUtc",
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

        Assert.Equal(expected, RunRecordSchema.FieldMap);
    }
}