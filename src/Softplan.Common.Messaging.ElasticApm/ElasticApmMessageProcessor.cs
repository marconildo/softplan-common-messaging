using System;
using System.Diagnostics;
using System.Reflection;
using Elastic.Apm.Api;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.ElasticApm
{
    public class ElasticApmMessageProcessor : IMessageProcessor
    {
        private static string TransactionType => "ElasticProccessMessage";
        private readonly ITracer _elasticApmtracer;

        public ElasticApmMessageProcessor(ITracer elasticApmtracer = null)
        {
            _elasticApmtracer = elasticApmtracer ?? new ElasticApmtracer();
        }

        public void ProcessMessage(IMessage message, IPublisher publisher, Action<IMessage, IPublisher> processMessage)
        {            
            var method = new StackFrame().GetMethod();
            var name = GetName(message, method, processMessage.GetMethodInfo().Name);
            var traceAsyncTransaction = GetTraceAsyncTransaction(message);
            if (message.Headers.ContainsKey(ApmConstants.TraceParent) && traceAsyncTransaction)
            {
                var traceParent = message.Headers[ApmConstants.TraceParent].ToString();
                _elasticApmtracer.CaptureTransaction(name, TransactionType, () => processMessage(message, publisher), DistributedTracingData.TryDeserializeFromString(traceParent));                                
            }
            else
            {
                _elasticApmtracer.CaptureTransaction(name, TransactionType, () => processMessage(message, publisher));
            }
        }        

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error, Func<IMessage, IPublisher, Exception, bool> handleProcessError)
        {            
            var method = new StackFrame().GetMethod();
            var name = GetName(message, method, handleProcessError.GetMethodInfo().Name);
            if (!message.Headers.ContainsKey(ApmConstants.TraceParent))
                return _elasticApmtracer.CaptureTransaction(name, TransactionType,() => handleProcessError(message, publisher, error));
            var traceParent = message.Headers[ApmConstants.TraceParent].ToString();
            return _elasticApmtracer.CaptureTransaction(name, TransactionType, () => handleProcessError(message, publisher, error), DistributedTracingData.TryDeserializeFromString(traceParent));
        }
        
        private static string GetName(IMessage message, MemberInfo method, string actionName)
        {            
            var name = message.Headers.ContainsKey(ApmConstants.TransactionName)
                ? message.Headers[ApmConstants.TransactionName].ToString()
                : $"{method.DeclaringType}.{method.Name}.{actionName}";
            return name;
        }
        
        private static bool GetTraceAsyncTransaction(IMessage message)
        {
            var traceAsyncTransaction = false;
            if (message.Headers.ContainsKey(ApmConstants.ApmTraceAsyncTransaction))
            {
                traceAsyncTransaction = (bool) message.Headers[ApmConstants.ApmTraceAsyncTransaction];                               
            }            
            return traceAsyncTransaction;
        }
    }
}