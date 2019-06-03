using System;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.Tests.TestProcessors
{
    public class InvalidConstructorProcessor : IProcessor
    {
        private readonly int _invalidArg;

        public InvalidConstructorProcessor(int invalidArg)
        {
            _invalidArg = invalidArg;
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
