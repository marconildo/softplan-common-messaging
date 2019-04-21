using System;
using Softplan.Common.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Softplan.Common.Messaging.UnitTest
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
