using System;
using System.Collections.Generic;

namespace Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces
{
    public interface IBuilder
    {
        IDictionary<string, Type> MessageQueueMap { get; }
        IPublisher BuildPublisher();
        IConsumer BuildConsumer();
        ISerializer BuildSerializer();
        IMessage BuildMessage(string queue, int version, string data = null);
        IQueueApiManager BuildApiManager();
    }
}
