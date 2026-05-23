namespace HALF.Host.Commands;

internal sealed class StatusCommand : IHostCommand
{
    public CommandDescriptor Descriptor => new("status", "Report the current runtime and observability status.");

    public CommandExecutionResult Execute(string[] args) =>
        new(0, "Status command is scaffolded through HALF.Host but not implemented yet.");
}