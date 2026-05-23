namespace HALF.Host.Commands;

internal sealed class BenchmarkCommand : IHostCommand
{
    public CommandDescriptor Descriptor => new("benchmark", "Capture benchmark evidence for a local runtime.");

    public CommandExecutionResult Execute(string[] args) =>
        new(0, "Benchmark command is scaffolded through HALF.Host but not implemented yet.");
}