namespace HALF.Host.Commands;

internal sealed class TraceCommand : IHostCommand
{
    public CommandDescriptor Descriptor => new("trace", "Inspect or export recorded traces for prior executions.");

    public CommandExecutionResult Execute(string[] args) =>
        new(0, "Trace command is scaffolded through HALF.Host but not implemented yet.");
}