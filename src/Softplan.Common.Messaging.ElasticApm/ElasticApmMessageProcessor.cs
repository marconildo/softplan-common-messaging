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
            var name = $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}.{message.ReplyQueue}";
            if (message.Headers.ContainsKey(ElasticApmConstants.TraceParent))
            {
                var traceParent = message.Headers[ElasticApmConstants.TraceParent].ToString();
                Agent.Tracer.CaptureTransaction(name, TransactionType,
                    () => processMessage(message, publisher),
                    DistributedTracingData.TryDeserializeFromString(traceParent));                                
            }
            else
            {
                Agent.Tracer.CaptureTransaction(name, TransactionType, 
                    () => processMessage(message, publisher));
            }
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error, Func<IMessage, IPublisher, Exception, bool> handleProcessError)
        {            
            var name = $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}.{message.ReplyQueue}";
            if (!message.Headers.ContainsKey(ElasticApmConstants.TraceParent))
                return Agent.Tracer.CaptureTransaction(name, TransactionType,
                    () => handleProcessError(message, publisher, error));
            var traceParent = message.Headers[ElasticApmConstants.TraceParent].ToString();
            return Agent.Tracer.CaptureTransaction(name, TransactionType, 
                () => handleProcessError(message, publisher, error),
                DistributedTracingData.TryDeserializeFromString(traceParent));
        }
    }
}