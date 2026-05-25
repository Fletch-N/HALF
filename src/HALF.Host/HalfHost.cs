using HALF.Host.Commands;

namespace HALF.Host;

public static class HalfHost
{

    public static IHalfHostRuntime CreateRuntime(HalfHostConfiguration? configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new HalfHostRuntime(
            configuration,
        [
            new RunCommand(),
            new BenchmarkCommand(),
            new TraceCommand(),
            new StatusCommand()
        ]);
    }
}

internal sealed class HalfHostRuntime(
    HalfHostConfiguration configuration,
    IReadOnlyList<IHostCommand> commands) : IHalfHostRuntime
{
    public HalfHostConfiguration Configuration { get; } = configuration;

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