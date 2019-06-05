using Microsoft.Extensions.Configuration;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IMessagingWorkersFactory
    {
        IMessageProcessor GetMessageProcessor(IConfiguration config);
        IMessagePublisher GetMessagePublisher(IConfiguration config);
    }
}