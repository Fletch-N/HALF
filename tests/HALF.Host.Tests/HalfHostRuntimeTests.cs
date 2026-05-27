using HALF.Watch;
using Microsoft.Extensions.DependencyInjection;

namespace HALF.Host.Tests;

public sealed class HalfHostRuntimeTests
{
    [Fact]
    public void Build_WithConfiguration_ExposesProvidedConfiguration()
    {
        var configuration = CreateConfiguration();

        using var runtime = HalfHost.Build(configuration);

        Assert.Equal(configuration, runtime.Configuration);
    }

    [Fact]
    public void Build_ExposesExpectedCommands()
    {
        var configuration = CreateConfiguration();

        using var runtime = HalfHost.Build(configuration);

        var commands = runtime.Commands;

        Assert.Collection(
            commands,
            command => Assert.Equal(new CommandDescriptor("run", "Execute a local model run through the host runtime."), command),
            command => Assert.Equal(new CommandDescriptor("benchmark", "Capture benchmark evidence for a local runtime."), command),
            command => Assert.Equal(new CommandDescriptor("trace", "Inspect or export recorded traces for prior executions."), command),
            command => Assert.Equal(new CommandDescriptor("status", "Report the current runtime and observability status."), command));
    }

    [Fact]
    public void RegisterHostServices_RegistersRuntimeConfigurationAndCommands()
    {
        var configuration = CreateConfiguration();
        var services = HalfHost.RegisterHostServices(new ServiceCollection(), configuration);

        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var registeredConfiguration = serviceProvider.GetRequiredService<HalfHostConfiguration>();
        var repository = serviceProvider.GetRequiredService<IRunRecordRepository>();
        var commands = serviceProvider.GetServices<IHostCommand>()
            .Select(command => command.Descriptor)
            .ToArray();

        Assert.Same(configuration, registeredConfiguration);
        Assert.NotNull(repository);
        Assert.Collection(
            commands,
            command => Assert.Equal(new CommandDescriptor("run", "Execute a local model run through the host runtime."), command),
            command => Assert.Equal(new CommandDescriptor("benchmark", "Capture benchmark evidence for a local runtime."), command),
            command => Assert.Equal(new CommandDescriptor("trace", "Inspect or export recorded traces for prior executions."), command),
            command => Assert.Equal(new CommandDescriptor("status", "Report the current runtime and observability status."), command));
    }

    [Theory]
    [InlineData("run")]
    [InlineData("RUN")]
    public void Execute_RunCommand_PersistsRunMetadata(string commandName)
    {
        var configuration = CreateConfiguration();
        using var runtime = HalfHost.Build(configuration);

        var result = runtime.Execute(commandName, []);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("metadata persisted", result.Message, StringComparison.OrdinalIgnoreCase);

        var trace = runtime.Execute("trace", []);

        Assert.Equal(0, trace.ExitCode);
        Assert.Contains("request_id=", trace.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Execute_BenchmarkCommand_ReturnsScaffoldResult()
    {
        var configuration = CreateConfiguration();
        using var runtime = HalfHost.Build(configuration);

        var result = runtime.Execute("benchmark", []);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("Benchmark command is scaffolded through HALF.Host but not implemented yet.", result.Message);
    }

    [Fact]
    public void Execute_StatusCommand_ReturnsRuntimeSummary()
    {
        var configuration = CreateConfiguration();
        using var runtime = HalfHost.Build(configuration);

        var result = runtime.Execute("status", []);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("HALF Host Status", result.Message, StringComparison.Ordinal);
        Assert.Contains("runs_total=", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_TraceCommand_ReturnsEmptyMessageWhenNoRunsExist()
    {
        var configuration = CreateConfiguration();
        using var runtime = HalfHost.Build(configuration);

        var result = runtime.Execute("trace", []);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("No persisted runs found.", result.Message);
    }

    [Fact]
    public void Execute_UnknownCommand_ReturnsFailureResult()
    {
        var configuration = CreateConfiguration();
        using var runtime = HalfHost.Build(configuration);

        var result = runtime.Execute("unknown", []);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("Unknown host command: unknown", result.Message);
    }

    private static HalfHostConfiguration CreateConfiguration()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), "HALF", "tests", Guid.NewGuid().ToString("N"));

        return new HalfHostConfiguration(
            new HalfHostRuntimeOptions(
                new Uri("http://192.168.1.20:11434"),
                "ollama-lan",
                "phi4-mini:3.8b",
                "q4_k_m"),
            new HalfHostStorageOptions(
                Path.Combine(testRoot, "state", "half.sqlite"),
                Path.Combine(testRoot, "state", "jsonl")),
            new HalfHostTelemetryOptions(
                false,
                "HALF.Host.Tests",
                "HALF.Host.Tests"));
    }
}