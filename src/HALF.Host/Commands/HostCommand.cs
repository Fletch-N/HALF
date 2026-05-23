namespace HALF.Host.Commands;

internal abstract class HostCommand : IHostCommand
{
    public abstract CommandDescriptor Descriptor { get; }

    public abstract CommandExecutionResult Execute(string[] args);
}