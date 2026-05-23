using HALF.Host;

namespace HALF.Host.Tests;

public sealed class HalfHostRuntimeTests
{
    [Fact]
    public void CreateDefault_ExposesExpectedCommands()
    {
        var runtime = HalfHost.CreateDefault();

        var commands = runtime.Commands;

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
        var runtime = HalfHost.CreateDefault();

        var result = runtime.Execute(commandName, []);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expectedMessage, result.Message);
    }

    [Fact]
    public void Execute_UnknownCommand_ReturnsFailureResult()
    {
        var runtime = HalfHost.CreateDefault();

        var result = runtime.Execute("unknown", []);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("Unknown host command: unknown", result.Message);
    }
}