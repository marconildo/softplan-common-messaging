using System;
using System.Collections.Generic;

namespace Softplan.Common.Messaging.Abstractions
{
    public interface IBuilder
    {
        IDictionary<string, Type> MessageQueueMap { get; }
        IPublisher BuildPublisher();
        IConsumer BuildConsumer();
        ISerializer BuildSerializer();
        IMessage BuildMessage(string queue, int version, string data = null);
        IQueueApiManager BuildAPIManager();
    }
}
