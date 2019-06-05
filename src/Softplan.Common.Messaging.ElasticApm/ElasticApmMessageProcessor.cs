using System;
using System.Reflection;
using Elastic.Apm;
using Elastic.Apm.Api;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.ElasticApm.Constants;

namespace Softplan.Common.Messaging.ElasticApm
{
    public class ElasticApmMessageProcessor : IMessageProcessor
    {         
        public static string TransactionType => "ElasticProccessMessage";
        
        public void ProcessMessage(IMessage message, IPublisher publisher, Action<IMessage, IPublisher> processMessage)
        {
            var traceParent = message.Headers[ElasticApmConstants.TraceParent].ToString();
            var name = $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}.{message.ReplyQueue}";            
            Agent.Tracer.CaptureTransaction(name, TransactionType, 
                () => processMessage(message, publisher),
                DistributedTracingData.TryDeserializeFromString(traceParent));
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error, Func<IMessage, IPublisher, Exception, bool> handleProcessError)
        {
            var traceParent = message.Headers[ElasticApmConstants.TraceParent].ToString();
            var name = $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}.{message.ReplyQueue}";            
            return Agent.Tracer.CaptureTransaction(name, TransactionType, 
                () => handleProcessError(message, publisher, error),
                DistributedTracingData.TryDeserializeFromString(traceParent));
        }
    }
}