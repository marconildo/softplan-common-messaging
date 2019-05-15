using System;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Abstractions;

namespace Softplan.Common.Messaging.Tests.TestProcessors
{
    public class TestProcessor : IProcessor
    {
        public ILogger Logger { get; set; }

        public TestProcessor(bool valid = true)
        {
            //test for default args
        }

        public Type GetMessageType()
        {
            return typeof(Message);
        }

        public string GetQueueName()
        {
            return String.Empty;
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
