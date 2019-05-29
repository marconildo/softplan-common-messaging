using System;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.Tests.TestProcessors
{
    public abstract class AbstractTestProcessor : IProcessor
    {
        public ILogger Logger { get; set; }

        public abstract Type GetMessageType();

        public abstract string GetQueueName();

        public abstract bool HandleProcessError(IMessage message, IPublisher publisher, Exception error);

        public abstract void ProcessMessage(IMessage message, IPublisher publisher);
    }
}
