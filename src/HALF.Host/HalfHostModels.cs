namespace HALF.Host;

public sealed record HalfHostConfiguration(
    HalfHostRuntimeOptions Runtime,
    HalfHostStorageOptions Storage,
    HalfHostTelemetryOptions Telemetry);

public sealed record HalfHostRuntimeOptions(
    Uri Endpoint,
    string RuntimeName,
    string ModelName,
    string? Quantization = null);

public sealed record HalfHostStorageOptions(
    string SqlitePath,
    string JsonlDirectoryPath);

public sealed record HalfHostTelemetryOptions(
    bool IsEnabled,
    string ActivitySourceName,
    string MeterName);

public sealed record CommandExecutionResult(int ExitCode, string Message);

public sealed record CommandDescriptor(string Name, string Description);

public interface IHalfHostRuntime
{
    CommandExecutionResult Execute(string commandName, string[] args);
    IReadOnlyList<CommandDescriptor> Commands { get; }
    HalfHostConfiguration Configuration { get; }
}

public interface IHostCommand
{
    CommandDescriptor Descriptor { get; }

    CommandExecutionResult Execute(string[] args);

}