using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MessageHandler : IHandleMessages<MyMessage>
{
    static readonly ILog log = LogManager.GetLogger<MessageHandler>();

    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        log.Info("Hello from Instance 1");
        return Task.CompletedTask;
    }
}