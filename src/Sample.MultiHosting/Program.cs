using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;


Console.Title = "Samples.MultiHosting";

#region multi-hosting-startup

using var endpointOneBuilder = ConfigureEndpointOne(Host.CreateApplicationBuilder(args)).Build();
using var endpointTwoBuilder = ConfigureEndpointTwo(Host.CreateApplicationBuilder(args)).Build();

await Task.WhenAll(endpointOneBuilder.StartAsync(), endpointTwoBuilder.StartAsync()).ConfigureAwait(true);
await Task.WhenAll(endpointOneBuilder.WaitForShutdownAsync(), endpointTwoBuilder.WaitForShutdownAsync()).ConfigureAwait(true);

#endregion

static HostApplicationBuilder ConfigureEndpointOne(HostApplicationBuilder builder)
{
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging")).AddConsole();

    #region multi-hosting-assembly-scan

    var endpointConfiguration = new EndpointConfiguration("Instance1");
    var scanner = endpointConfiguration.AssemblyScanner();
    scanner.ExcludeAssemblies("Instance2");

    #endregion

    endpointConfiguration.UseSerialization<SystemJsonSerializer>();
    endpointConfiguration.UseTransport(new LearningTransport());
    endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

    builder.UseNServiceBus(endpointConfiguration);

    return builder;
}

static HostApplicationBuilder ConfigureEndpointTwo(HostApplicationBuilder builder)
{
    var logging = builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();

    var endpointConfiguration = new EndpointConfiguration("Instance2");
    var scanner = endpointConfiguration.AssemblyScanner();
    scanner.ExcludeAssemblies("Instance1");

    endpointConfiguration.UseSerialization<SystemJsonSerializer>();
    endpointConfiguration.UseTransport(new LearningTransport());
    endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

    builder.UseNServiceBus(endpointConfiguration);

    return builder;
}

static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
{
    var fatalMessage = "The following critical error was " +
                       $"encountered: {Environment.NewLine}{context.Error}{Environment.NewLine}Process is shutting down. " +
                       $"StackTrace: {Environment.NewLine}{context.Exception.StackTrace}";

    try
    {
        await context.Stop(cancellationToken).ConfigureAwait(false);
    }
    finally
    {
        Environment.FailFast(fatalMessage, context.Exception);
    }
}

