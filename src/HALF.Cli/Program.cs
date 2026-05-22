namespace HALF.Cli;

internal interface ICommandHandler
{
    string CommandName { get; }
    int Execute(string[] args);
}

internal sealed class RunCommandHandler : ICommandHandler
{
    public string CommandName => "run";

    public int Execute(string[] args)
    {
        Console.WriteLine("Run command is scaffolded but not implemented yet.");
        Console.WriteLine("Focus remains on building the observability-first runtime baseline.");
        return 0;
    }
}

internal sealed class BenchmarkCommandHandler : ICommandHandler
{
    public string CommandName => "benchmark";

    public int Execute(string[] args)
    {
        Console.WriteLine("Benchmark command is scaffolded but not implemented yet.");
        Console.WriteLine("Focus remains on building the observability-first runtime baseline.");
        return 0;
    }
}

internal sealed class TraceCommandHandler : ICommandHandler
{
    public string CommandName => "trace";

    public int Execute(string[] args)
    {
        Console.WriteLine("Trace command is scaffolded but not implemented yet.");
        Console.WriteLine("Focus remains on building the observability-first runtime baseline.");
        return 0;
    }
}

internal sealed class StatusCommandHandler : ICommandHandler
{
    public string CommandName => "status";

    public int Execute(string[] args)
    {
        Console.WriteLine("Status command is scaffolded but not implemented yet.");
        Console.WriteLine("Focus remains on building the observability-first runtime baseline.");
        return 0;
    }
}

internal static class Program
{
    private enum CliCommand
    {
        Run,
        Benchmark,
        Trace,
        Status
    }

    private static bool TryParseCommand(string value, out CliCommand command) =>
        Enum.TryParse(value, ignoreCase: true, out command);

    private static int Main(string[] args)
    {


        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp();
            return 0;
        }

        if (IsVersion(args[0]))
        {
            Console.WriteLine("HALF CLI 0.1.0-scaffold");
            return 0;
        }

        switch (TryParseCommand(args[0], out var command))
        {
            case true when command == CliCommand.Run:
                return new RunCommandHandler().Execute(args[1..]);
            case true when command == CliCommand.Benchmark:
                return new BenchmarkCommandHandler().Execute(args[1..]);
            case true when command == CliCommand.Trace:
                return new TraceCommandHandler().Execute(args[1..]);
            case true when command == CliCommand.Status:
                return new StatusCommandHandler().Execute(args[1..]);
        }

        Console.Error.WriteLine($"Unknown command: {args[0]}");
        Console.Error.WriteLine("Run 'half help' to inspect the current scaffold.");
        return 1;
    }

    private static bool IsHelp(string argument) =>
        string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase);

    private static bool IsVersion(string argument) =>
        string.Equals(argument, "version", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "--version", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "-v", StringComparison.OrdinalIgnoreCase);

    private static void PrintHelp()
    {
        Console.WriteLine("HALF CLI");
        Console.WriteLine("Humble Agentic Lightweight Framework");
        Console.WriteLine();
        Console.WriteLine("Scaffolded commands:");

        foreach (var command in Enum.GetNames<CliCommand>())
        {
            if (command != null)
                Console.WriteLine($"{command.ToLower()}");
        }

        Console.WriteLine();
        Console.WriteLine("Current focus: observability-first local runtime integration with Ollama.");
    }
}
