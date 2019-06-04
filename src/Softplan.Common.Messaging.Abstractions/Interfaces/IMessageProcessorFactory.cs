using Microsoft.Extensions.Configuration;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IMessageProcessorFactory
    {
        IMessageProcessor GetMessageProcessor(IConfiguration config);
    }
}