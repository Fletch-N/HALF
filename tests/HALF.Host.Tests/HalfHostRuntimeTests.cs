using HALF.Host;
using Microsoft.Extensions.DependencyInjection;

namespace HALF.Host.Tests;

public sealed class HalfHostRuntimeTests
{
    private readonly HalfHostConfiguration configuration = new(
            new HalfHostRuntimeOptions(
                new Uri("http://192.168.1.20:11434"),
                "ollama-lan",
                "phi4-mini:3.8b",
                "q4_k_m"),
            new HalfHostStorageOptions(
                "state/half.sqlite",
                "state/jsonl"),
            new HalfHostTelemetryOptions(
                false,
                "HALF.Host.Tests",
                "HALF.Host.Tests"));

    [Fact]
    public void Build_WithConfiguration_ExposesProvidedConfiguration()
    {
        using var runtime = HalfHost.Build(configuration);

        Assert.Equal(configuration, runtime.Configuration);
    }

    [Fact]
    public void Build_ExposesExpectedCommands()
    {
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
        var services = HalfHost.RegisterHostServices(new ServiceCollection(), configuration);

        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var registeredConfiguration = serviceProvider.GetRequiredService<HalfHostConfiguration>();
        var commands = serviceProvider.GetServices<IHostCommand>()
            .Select(command => command.Descriptor)
            .ToArray();

        Assert.Same(configuration, registeredConfiguration);
        Assert.Collection(
            commands,
            command => Assert.Equal(new CommandDescriptor("run", "Execute a local model run through the host runtime."), command),
            command => Assert.Equal(new CommandDescriptor("benchmark", "Capture benchmark evidence for a local runtime."), command),
            command => Assert.Equal(new CommandDescriptor("trace", "Inspect or export recorded traces for prior executions."), command),
            command => Assert.Equal(new CommandDescriptor("status", "Report the current runtime and observability status."), command));
    }

    [Theory]
    [InlineData("run", "Run command is scaffolded through HALF.Host but not implemented yet.")]
    [InlineData("RUN", "Run command is scaffolded through HALF.Host but not implemented yet.")]
    [InlineData("benchmark", "Benchmark command is scaffolded through HALF.Host but not implemented yet.")]
    [InlineData("trace", "Trace command is scaffolded through HALF.Host but not implemented yet.")]
    [InlineData("status", "Status command is scaffolded through HALF.Host but not implemented yet.")]
    public void Execute_KnownCommand_ReturnsSuccessfulScaffoldResult(string commandName, string expectedMessage)
    {
        using var runtime = HalfHost.Build(configuration);

        var result = runtime.Execute(commandName, []);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expectedMessage, result.Message);
    }

    [Fact]
    public void Execute_UnknownCommand_ReturnsFailureResult()
    {
        using var runtime = HalfHost.Build(configuration);

        var result = runtime.Execute("unknown", []);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("Unknown host command: unknown", result.Message);
    }
}