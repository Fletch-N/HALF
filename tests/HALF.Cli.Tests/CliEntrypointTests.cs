using System.Reflection;

namespace HALF.Cli.Tests;

public sealed class CliEntrypointTests
{
    private static readonly Lock ConsoleLock = new();

    [Fact]
    public void Main_NoArguments_PrintsHelpAndReturnsZero()
    {
        var result = InvokeMain([]);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("HALF CLI", result.Output);
        Assert.Contains("Scaffolded commands:", result.Output);
        Assert.Contains("run - Execute a local model run through the host runtime.", result.Output);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("--help")]
    [InlineData("-h")]
    public void Main_HelpAliases_PrintHelpAndReturnZero(string argument)
    {
        var result = InvokeMain([argument]);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("HALF CLI", result.Output);
        Assert.Contains("benchmark - Capture benchmark evidence for a local runtime.", result.Output);
    }

    [Theory]
    [InlineData("version")]
    [InlineData("--version")]
    [InlineData("-v")]
    public void Main_VersionAliases_PrintVersionAndReturnZero(string argument)
    {
        var result = InvokeMain([argument]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal($"HALF CLI 0.1.0-scaffold{Environment.NewLine}", result.Output);
    }

    [Fact]
    public void Main_KnownCommand_ForwardsToHost()
    {
        var result = InvokeMain(["trace"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(
            $"Trace command is scaffolded through HALF.Host but not implemented yet.{Environment.NewLine}",
            result.Output);
    }

    [Fact]
    public void Main_UnknownCommand_ReturnsHostFailure()
    {
        var result = InvokeMain(["unknown"]);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal($"Unknown host command: unknown{Environment.NewLine}", result.Output);
    }

    private static CliInvocationResult InvokeMain(string[] args)
    {
        lock (ConsoleLock)
        {
            var assembly = Assembly.Load("HALF.Cli");
            var programType = assembly.GetType("HALF.Cli.Program", throwOnError: true)!;
            var mainMethod = programType.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.NotNull(mainMethod);

            using var writer = new StringWriter();

            var originalOut = Console.Out;

            try
            {
                Console.SetOut(writer);

                var exitCode = (int?)mainMethod!.Invoke(null, [args]);

                Assert.NotNull(exitCode);

                return new CliInvocationResult(exitCode.Value, writer.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }

    private sealed record CliInvocationResult(int ExitCode, string Output);
}