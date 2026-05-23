namespace HALF.Host.Commands;

internal sealed class RunCommand : IHostCommand
{
    public CommandDescriptor Descriptor => new("run", "Execute a local model run through the host runtime.");

    public CommandExecutionResult Execute(string[] args) =>
        new(0, "Run command is scaffolded through HALF.Host but not implemented yet.");
}