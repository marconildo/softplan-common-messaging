using System;
using Softplan.Common.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Softplan.Common.Messaging.UnitTest.TestProcessors
{
    public class InvalidConstructorProcessor : IProcessor
    {
        private readonly int invalidArg;

        public InvalidConstructorProcessor(int invalidArg)
        {
            this.invalidArg = invalidArg;
        }

        public ILogger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Type GetMessageType()
        {
            throw new NotImplementedException();
        }

        public string GetQueueName()
        {
            throw new NotImplementedException();
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
