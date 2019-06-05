using System;
using System.Reflection;
using System.Threading.Tasks;
using Elastic.Apm;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.ElasticApm.Constants;

namespace Softplan.Common.Messaging.ElasticApm
{
    public class ElasticApmMessagePublisher : IMessagePublisher
    {
        private static string TransactionType => "ElasticCreateTransactionMessage";
        private static string SpanType => "ElasticPublishMessage";
        
        public void Publish(IMessage message, string destination, bool forceDestination, Action<IMessage, string, bool> publish)
        {
            var name = $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}.{message.ReplyQueue}";
            var transaction = Agent.Tracer.CurrentTransaction;            
            
            if (transaction != null)
            {                 
                var traceParent = $"00-{transaction.TraceId}-{transaction.Id}-01";
                message.Headers[ElasticApmConstants.TraceParent] = traceParent;                 
                transaction.CaptureSpan(name, SpanType,
                    () => publish(message, destination, forceDestination));
            }
            else
            {
                Agent.Tracer.CaptureTransaction(name, TransactionType, 
                    () => Publish(message, destination, forceDestination, publish));
            }
        }

        public async Task<T> PublishAndWait<T>(IMessage message, string destination, bool forceDestination, int milliSecondsTimeout,
            Func<IMessage, string, bool, int, Task<T>> publishAndWait) where T : IMessage
        {
            var name = $"{GetType().Name}.{MethodBase.GetCurrentMethod().Name}.{message.ReplyQueue}";
            var transaction = Agent.Tracer.CurrentTransaction; 
            
            if (transaction != null)
            {                 
                var traceParent = $"00-{transaction.TraceId}-{transaction.Id}-01";
                message.Headers[ElasticApmConstants.TraceParent] = traceParent;                 
                return await  transaction.CaptureSpan(name, SpanType,
                    async () => await  publishAndWait(message, destination, forceDestination, milliSecondsTimeout));
            }

            return await Agent.Tracer.CaptureTransaction(name, TransactionType, 
                async () => await PublishAndWait(message, destination, forceDestination, milliSecondsTimeout, publishAndWait));
        }
    }
}