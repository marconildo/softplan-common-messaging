using Microsoft.Extensions.Logging;
using System;

namespace Softplan.Common.Messaging.Abstractions
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
