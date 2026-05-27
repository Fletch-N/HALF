using System.Reflection;
using HALF.Host;
using Microsoft.Extensions.Configuration;

namespace HALF.Cli.Tests;

public sealed class CliEntrypointTests
{
    private static readonly Lock ConsoleLock = new();

    [Fact]
    public void Main_NoArguments_PrintsHelpAndReturnsZero()
    {
        var result = InvokeMain([]);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("HALF CLI", result.Output);
        Assert.Contains("Scaffolded commands:", result.Output);
        Assert.Contains("run - Execute a local model run through the host runtime.", result.Output);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("--help")]
    [InlineData("-h")]
    public void Main_HelpAliases_PrintHelpAndReturnZero(string argument)
    {
        var result = InvokeMain([argument]);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("HALF CLI", result.Output);
        Assert.Contains("benchmark - Capture benchmark evidence for a local runtime.", result.Output);
    }

    [Theory]
    [InlineData("version")]
    [InlineData("--version")]
    [InlineData("-v")]
    public void Main_VersionAliases_PrintVersionAndReturnZero(string argument)
    {
        var result = InvokeMain([argument]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal($"HALF CLI 0.1.0-scaffold{Environment.NewLine}", result.Output);
    }

    [Fact]
    public void Main_KnownCommand_ForwardsToHost()
    {
        var result = InvokeMain(["trace"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal($"No persisted runs found.{Environment.NewLine}", result.Output);
    }

    [Fact]
    public void Main_UnknownCommand_ReturnsHostFailure()
    {
        var result = InvokeMain(["unknown"]);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal($"Unknown host command: unknown{Environment.NewLine}", result.Output);
    }

    [Fact]
    public void LoadHostConfiguration_UsesProvidedFlatConfigurationValues()
    {
        lock (ConsoleLock)
        {
            const string basePath = "C:\\HALF\\config";

            var configuration = InvokeLoadHostConfiguration(
                CreateConfiguration(
                [
                    new KeyValuePair<string, string?>("Runtime:Endpoint", "http://ollama:11434"),
                    new KeyValuePair<string, string?>("Runtime:RuntimeName", "ollama-prod"),
                    new KeyValuePair<string, string?>("Runtime:ModelName", "granite4.1:3b"),
                    new KeyValuePair<string, string?>("Runtime:Quantization", "q4_k_m"),
                    new KeyValuePair<string, string?>("Storage:SqlitePath", "data/prod/half.db"),
                    new KeyValuePair<string, string?>("Storage:JsonlDirectoryPath", "data/prod/jsonl"),
                    new KeyValuePair<string, string?>("Telemetry:IsEnabled", "true"),
                    new KeyValuePair<string, string?>("Telemetry:ActivitySourceName", "HALF.Host.Prod"),
                    new KeyValuePair<string, string?>("Telemetry:MeterName", "HALF.Host.Prod")
                ]),
                basePath);

            Assert.Equal(
                new HalfHostConfiguration(
                    new HalfHostRuntimeOptions(
                        new Uri("http://ollama:11434"),
                        "ollama-prod",
                        "granite4.1:3b",
                        "q4_k_m"),
                    new HalfHostStorageOptions(
                        "data/prod/half.db",
                        "data/prod/jsonl"),
                    new HalfHostTelemetryOptions(
                        true,
                        "HALF.Host.Prod",
                        "HALF.Host.Prod")),
                configuration);
        }
    }

    [Fact]
    public void LoadHostConfiguration_UsesCurrentDefaultsWhenConfigurationValuesAreMissing()
    {
        lock (ConsoleLock)
        {
            var basePath = Path.Combine("C:\\HALF", "workspace");

            var configuration = InvokeLoadHostConfiguration(new ConfigurationBuilder().Build(), basePath);

            Assert.Equal(new Uri("http://localhost:11434"), configuration.Runtime.Endpoint);
            Assert.Equal("ollama", configuration.Runtime.RuntimeName);
            Assert.Equal("qwen3.5:4b", configuration.Runtime.ModelName);
            Assert.Null(configuration.Runtime.Quantization);
            Assert.Equal(Path.Combine(basePath, "data/half.db"), configuration.Storage.SqlitePath);
            Assert.Equal(Path.Combine(basePath, "data/jsonl"), configuration.Storage.JsonlDirectoryPath);
            Assert.False(configuration.Telemetry.IsEnabled);
            Assert.Equal("HALF.Host", configuration.Telemetry.ActivitySourceName);
            Assert.Equal("HALF.Host", configuration.Telemetry.MeterName);
        }
    }

    private static CliInvocationResult InvokeMain(string[] args)
    {
        lock (ConsoleLock)
        {
            var assembly = Assembly.Load("HALF.Cli");
            var programType = assembly.GetType("HALF.Cli.Program", throwOnError: true)!;
            var mainMethod = programType.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.NotNull(mainMethod);

            using var writer = new StringWriter();

            var originalOut = Console.Out;

            try
            {
                Console.SetOut(writer);

                var exitCode = (int?)mainMethod!.Invoke(null, [args]);

                Assert.NotNull(exitCode);

                return new CliInvocationResult(exitCode.Value, writer.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }

    private static HalfHostConfiguration InvokeLoadHostConfiguration(IConfiguration configuration, string basePath)
    {
        var assembly = Assembly.Load("HALF.Cli");
        var programType = assembly.GetType("HALF.Cli.Program", throwOnError: true)!;
        var loadMethod = programType.GetMethod(
            "LoadHostConfiguration",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(IConfiguration), typeof(string)],
            modifiers: null);

        Assert.NotNull(loadMethod);

        var hostConfiguration = (HalfHostConfiguration?)loadMethod!.Invoke(null, [configuration, basePath]);

        Assert.NotNull(hostConfiguration);
        return hostConfiguration;
    }

    private static IConfiguration CreateConfiguration(IEnumerable<KeyValuePair<string, string?>> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private sealed record CliInvocationResult(int ExitCode, string Output);
}