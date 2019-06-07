using System;
using System.Reflection;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
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
                PublishMessage(message, destination, forceDestination, publish, transaction, name);
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
                return await PublishMessageAndWait(message, destination, forceDestination, milliSecondsTimeout, publishAndWait, transaction, name);
            }
            return await Agent.Tracer.CaptureTransaction(name, TransactionType, 
                async () => await PublishAndWait(message, destination, forceDestination, milliSecondsTimeout, publishAndWait));
        }
        

        private static void PublishMessage(IMessage message, string destination, bool forceDestination, Action<IMessage, string, bool> publish,
            IExecutionSegment transaction, string name)
        {
            var span = transaction.StartSpan(name, SpanType);
            try
            {
                var traceParent = $"00-{transaction.TraceId}-{span.Id}-01";
                message.Headers[ElasticApmConstants.TraceParent] = traceParent;
                publish(message, destination, forceDestination);
            }
            catch (Exception ex)
            {
                span.CaptureException(ex);
                throw;
            }
            finally
            {
                span.End();
            }
        }
        
        private static async Task<T> PublishMessageAndWait<T>(IMessage message, string destination, bool forceDestination,
            int milliSecondsTimeout, Func<IMessage, string, bool, int, Task<T>> publishAndWait, IExecutionSegment transaction, string name) where T : IMessage
        {
            var span = transaction.StartSpan(name, SpanType);
            try
            {
                var traceParent = $"00-{transaction.TraceId}-{span.Id}-01";
                message.Headers[ElasticApmConstants.TraceParent] = traceParent;
                return await publishAndWait(message, destination, forceDestination, milliSecondsTimeout);
            }
            catch (Exception ex)
            {
                span.CaptureException(ex);
                throw;
            }
            finally
            {
                span.End();
            }
        }
    }
}