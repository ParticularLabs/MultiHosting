using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;


Console.Title = "Samples.MultiHosting";

using var endpointOneBuilder = ConfigureEndpointOne(Host.CreateDefaultBuilder(args)).Build();
using var endpointTwoBuilder = ConfigureEndpointTwo(Host.CreateDefaultBuilder(args)).Build();

await Task.WhenAll(endpointOneBuilder.StartAsync(), endpointTwoBuilder.StartAsync());
await Task.WhenAll(endpointOneBuilder.WaitForShutdownAsync(), endpointTwoBuilder.WaitForShutdownAsync());


static IHostBuilder ConfigureEndpointOne(IHostBuilder builder)
{
    builder.UseConsoleLifetime();
    builder.ConfigureLogging((ctx, logging) =>
    {
        logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
        logging.AddEventLog();
        logging.AddConsole();
    });

    builder.UseNServiceBus(ctx =>
    {
        var endpointConfiguration = new EndpointConfiguration("Instance1");
        var scanner = endpointConfiguration.AssemblyScanner();
        scanner.ExcludeAssemblies("Instance2");

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.UseTransport(new LearningTransport());
        endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

        return endpointConfiguration;
    });

    return builder;
}

static IHostBuilder ConfigureEndpointTwo(IHostBuilder builder)
{
    builder.UseConsoleLifetime();
    builder.ConfigureLogging((ctx, logging) =>
    {
        logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
        logging.AddEventLog();
        logging.AddConsole();
    });

    builder.UseNServiceBus(ctx =>
    {
        var endpointConfiguration = new EndpointConfiguration("Instance2");
        var scanner = endpointConfiguration.AssemblyScanner();
        scanner.ExcludeAssemblies("Instance1");
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.UseTransport(new LearningTransport());
        endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

        return endpointConfiguration;
    });

    return builder;
}

static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
{
    var fatalMessage = "The following critical error was " +
                       $"encountered: {Environment.NewLine}{context.Error}{Environment.NewLine}Process is shutting down. " +
                       $"StackTrace: {Environment.NewLine}{context.Exception.StackTrace}";

    try
    {
        await context.Stop(cancellationToken);
    }
    finally
    {
        Environment.FailFast(fatalMessage, context.Exception);
    }
}

