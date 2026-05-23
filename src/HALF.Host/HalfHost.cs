using HALF.Host.Commands;

namespace HALF.Host;

public static class HalfHost
{
    public static IHalfHostRuntime CreateDefault() =>
        new HalfHostRuntime(
        [
            new RunCommand(),
            new BenchmarkCommand(),
            new TraceCommand(),
            new StatusCommand()
        ]);
}

internal sealed class HalfHostRuntime(IReadOnlyList<IHostCommand> commands) : IHalfHostRuntime
{
    private readonly IReadOnlyList<IHostCommand> commands = commands;

    public IReadOnlyList<CommandDescriptor> Commands =>
        [.. commands.Select(command => command.Descriptor)];

    public CommandExecutionResult Execute(string commandName, string[] args)
    {
        var command = commands.FirstOrDefault(candidate =>
            string.Equals(candidate.Descriptor.Name, commandName, StringComparison.OrdinalIgnoreCase));

        return command is null
            ? new CommandExecutionResult(1, $"Unknown host command: {commandName}")
            : command.Execute(args);
    }
}