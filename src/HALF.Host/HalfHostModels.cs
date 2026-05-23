namespace HALF.Host;

public sealed record CommandExecutionResult(int ExitCode, string Message);

public sealed record CommandDescriptor(string Name, string Description);

public interface IHalfHostRuntime
{
    CommandExecutionResult Execute(string commandName, string[] args);
    IReadOnlyList<CommandDescriptor> Commands { get; }
}

public interface IHostCommand
{
    CommandDescriptor Descriptor { get; }

    CommandExecutionResult Execute(string[] args);

}