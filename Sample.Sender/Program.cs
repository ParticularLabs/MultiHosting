using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

var sendOnlyInstance = await StartSendOnlyEndpoint(CancellationToken.None);

try
{
    Console.WriteLine("Press '1' to send a message from this endpoint to Instance1");
    Console.WriteLine("Press '2' to send a message from this endpoint to Instance2");
    Console.WriteLine("Press any key to exit");

    while (true)
    {
        var key = Console.ReadKey();
        Console.WriteLine();
        var message = new MyMessage();
        if (key.Key == ConsoleKey.D1)
        {
            await sendOnlyInstance.Send("Instance1", message);
            continue;
        }
        if (key.Key == ConsoleKey.D2)
        {
            await sendOnlyInstance.Send("Instance2", message);
            continue;
        }
        return;
    }
}
finally
{
    if (sendOnlyInstance != null)
    {
        await sendOnlyInstance.Stop();
    }
}


static Task<IEndpointInstance> StartSendOnlyEndpoint(CancellationToken cancellationToken)
{
    var endpointConfiguration = new EndpointConfiguration("Samples.MultiHosting.SendOnly");
    _ = endpointConfiguration.AssemblyScanner();

    _ = endpointConfiguration.UsePersistence<LearningPersistence>();
    _ = endpointConfiguration.UseSerialization<SystemJsonSerializer>();
    _ = endpointConfiguration.UseTransport(new LearningTransport());

    return Endpoint.Start(endpointConfiguration, cancellationToken);
}

