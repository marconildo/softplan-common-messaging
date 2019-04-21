using System;
using Softplan.Common.Messaging;
using Softplan.Common.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Softplan.Common.Messaging.UnitTest.TestProcessors
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
