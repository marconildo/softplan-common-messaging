using System;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.Tests.TestProcessors
{
    public class TestProcessor : IProcessor
    {
        public ILogger Logger { get; set; }

        public Type GetMessageType()
        {
            return typeof(Message);
        }

        public string GetQueueName()
        {
            return string.Empty;
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error)
        {
            throw new NotImplementedException();
        }

        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            throw new NotImplementedException();
        }
    }
}
