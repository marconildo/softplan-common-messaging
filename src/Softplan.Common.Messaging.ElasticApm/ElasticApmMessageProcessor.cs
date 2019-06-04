using System;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.ElasticApm
{
    public class ElasticApmMessageProcessor : IMessageProcessor
    {
        public void ProcessMessage(IMessage message, IPublisher publisher, Action<IMessage, IPublisher> processMessage)
        {
            var traceParent = $"00-{message.MainOperationId}-{message.ParentOperationId}-01";
            throw new NotImplementedException();
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error, Func<IMessage, IPublisher, Exception, bool> handleProcessError)
        {
            throw new NotImplementedException();
        }
    }
}