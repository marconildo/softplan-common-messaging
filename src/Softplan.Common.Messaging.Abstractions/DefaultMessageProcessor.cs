using System;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.Abstractions
{
    public class DefaultMessageProcessor : IMessageProcessor
    {
        public void ProcessMessage(IMessage message, IPublisher publisher, Action<IMessage, IPublisher> processMessage)
        {
            processMessage(message, publisher);
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error, Func<IMessage, IPublisher, Exception, bool> handleProcessError)
        {
            return handleProcessError(message, publisher, error);
        }
    }
}