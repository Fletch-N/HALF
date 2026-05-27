using HALF.Host.Commands;
using HALF.Watch;
using Microsoft.Extensions.DependencyInjection;

namespace HALF.Host;

public static class HalfHost
{
    public static IHalfHostRuntime Build(HalfHostConfiguration? configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        ServiceProvider serviceProvider = RegisterHostServices(new ServiceCollection(), configuration)
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        return new HalfHostRuntime(
            configuration,
            serviceProvider,
            serviceProvider.GetServices<IHostCommand>());
    }

    public static IServiceCollection RegisterHostServices(IServiceCollection services, HalfHostConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddSingleton<IRunRecordRepository>(_ =>
        {
            var repository = new SqliteRunRecordRepository(configuration.Storage.SqlitePath);
            repository.EnsureInitialized();
            return repository;
        });

        services.AddSingleton<IHostCommand, RunCommand>();
        services.AddSingleton<IHostCommand, BenchmarkCommand>();
        services.AddSingleton<IHostCommand, TraceCommand>();
        services.AddSingleton<IHostCommand, StatusCommand>();

        return services;
    }
}

internal sealed class HalfHostRuntime(
    HalfHostConfiguration configuration,
    IServiceProvider serviceProvider,
    IEnumerable<IHostCommand> commands) : IHalfHostRuntime
{
    public HalfHostConfiguration Configuration { get; } = configuration;
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly IReadOnlyList<IHostCommand> commands = [.. commands];

    public IReadOnlyList<CommandDescriptor> Commands =>
        [.. commands.Select(command => command.Descriptor)];

    public CommandExecutionResult Execute(string commandName, string[] args)
    {
        var command = commands.FirstOrDefault(candidate =>
            string.Equals(candidate.Descriptor.Name, commandName, StringComparison.OrdinalIgnoreCase));

        return command is null
            ? new CommandExecutionResult(1, $"Unknown host command: {commandName}")
            : command.Execute(args);
    }

    public void Dispose()
    {
        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}