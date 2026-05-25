using Microsoft.Extensions.Configuration;
using HALF.Host;

namespace HALF.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables(prefix: "HALF_")
        .Build();

        HalfHostConfiguration hostConfig = LoadHostConfiguration(config.GetRequiredSection("Host"), AppContext.BaseDirectory);

        var host = HalfHost.CreateRuntime(hostConfig);

        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp(host);
            return 0;
        }

        if (IsVersion(args[0]))
        {
            Console.WriteLine("HALF CLI 0.1.0-scaffold");
            return 0;
        }

        var result = host.Execute(args[0], [.. args.Skip(1)]);

        Console.WriteLine(result.Message);
        return result.ExitCode;
    }

    private static HalfHostConfiguration LoadHostConfiguration(IConfiguration config, string basePath)
    {
        return new(
            new HalfHostRuntimeOptions(
                new Uri(config["Runtime:Endpoint"] ?? "http://localhost:11434"),
                config["Runtime:RuntimeName"] ?? "ollama",
                config["Runtime:ModelName"] ?? "qwen3.5:4b",
                config["Runtime:Quantization"]),
            new HalfHostStorageOptions(
                config["Storage:SqlitePath"] ?? Path.Combine(basePath, "data/half.db"),
                config["Storage:JsonlDirectoryPath"] ?? Path.Combine(basePath, "data/jsonl")),
            new HalfHostTelemetryOptions(
                bool.TryParse(config["Telemetry:IsEnabled"], out var isEnabled) && isEnabled,
                config["Telemetry:ActivitySourceName"] ?? "HALF.Host",
                config["Telemetry:MeterName"] ?? "HALF.Host"));
    }

    private static bool IsHelp(string argument) =>
        string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase);

    private static bool IsVersion(string argument) =>
        string.Equals(argument, "version", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "--version", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(argument, "-v", StringComparison.OrdinalIgnoreCase);

    private static void PrintHelp(IHalfHostRuntime host)
    {
        Console.WriteLine("HALF CLI");
        Console.WriteLine("Humble Agentic Lightweight Framework");
        Console.WriteLine();
        Console.WriteLine("Scaffolded commands:");

        foreach (var command in host.Commands)
        {
            Console.WriteLine($"  {command.Name} - {command.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Current focus: observability-first local runtime integration with Ollama.");
    }
}
