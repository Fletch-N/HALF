using HALF.Host;

namespace HALF.Cli;

internal static class Program
{
    private static readonly IHalfHostRuntime Host = HalfHost.CreateDefault();

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

        var result = Host.Execute(args[0], [.. args.Skip(1)]);

        Console.WriteLine(result.Message);
        return result.ExitCode;
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

        foreach (var command in Host.Commands)
        {
            Console.WriteLine($"  {command.Name} - {command.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Current focus: observability-first local runtime integration with Ollama.");
    }
}
