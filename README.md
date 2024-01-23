## Generic Host multiple endpoint hosting

> [!WARNING]
> The approach shown in this sample is not recommended. The recommended approach is to have one host and one endpoint per process.

This sample shows how to host multiple endpoints in one generic host process by using multiple `IHostBuilder` instances. When started, the application creates two host builder instances, each configured for a different endpoint that could be using different configurations:

```
using var endpointOneBuilder = ConfigureEndpointOne(Host.CreateDefaultBuilder(args)).Build();
using var endpointTwoBuilder = ConfigureEndpointTwo(Host.CreateDefaultBuilder(args)).Build();

await Task.WhenAll(endpointOneBuilder.StartAsync(), endpointTwoBuilder.StartAsync());
await Task.WhenAll(endpointOneBuilder.WaitForShutdownAsync(), endpointTwoBuilder.WaitForShutdownAsync());
```

An important thing to keep in mind is that [dependency injection](/nservicebus/dependency-injection/) is used internally to register components, handlers, and sagas. Each host has a separate ServiceProvider which means the containers are not shared between the endpoints.

To ensure that each endpoint instance registers only its own components like message handlers, it is important to specify an assembly scan policy using [one of the supported approaches](/nservicebus/hosting/assembly-scanning.md).

> [!WARNING]
> If a single endpoint fails to start, the host will shut down, terminating the other endpoint.

In this example, complete isolation is required between the two endpoints so that the types from Instance2 are excluded from Instance1 and vice versa.

```
   var endpointConfiguration = new EndpointConfiguration("Instance1");
   var scanner = endpointConfiguration.AssemblyScanner();
   scanner.ExcludeAssemblies("Instance2");
```
