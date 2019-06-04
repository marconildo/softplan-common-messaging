using System;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IMessageProcessor
    {
        void ProcessMessage(IMessage message, IPublisher publisher, Action<IMessage, IPublisher> processMessage);
        
        bool HandleProcessError(IMessage message, IPublisher publisher, Exception error, Func<IMessage, IPublisher, Exception, bool> handleProcessError);
    }
}