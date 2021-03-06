using System;
using Microsoft.Extensions.Logging;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IProcessor
    {
        ILogger Logger { get; set; }
        string GetQueueName();
        Type GetMessageType();
        void ProcessMessage(IMessage message, IPublisher publisher);
        bool HandleProcessError(IMessage message, IPublisher publisher, Exception error);
    }
}
