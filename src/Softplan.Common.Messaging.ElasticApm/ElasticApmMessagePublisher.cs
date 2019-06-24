using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.ElasticApm
{
    public class ElasticApmMessagePublisher : IMessagePublisher
    {
        private readonly IConfiguration _config;
        private static string TransactionType => "ElasticCreateTransactionMessage";
        private static string SpanType => "ElasticPublishMessage";
        private readonly ITracer _elasticApmtracer;

        public ElasticApmMessagePublisher(IConfiguration config, ITracer elasticApmtracer = null)
        {
            _config = config;
            _elasticApmtracer = elasticApmtracer ?? new ElasticApmtracer();
        }
        
        public void Publish(IMessage message, string destination, bool forceDestination, Action<IMessage, string, bool> publish)
        {
            var method = new StackFrame().GetMethod();
            var name = GetName(message, method, publish.GetMethodInfo().Name);
            var transaction = _elasticApmtracer.CurrentTransaction;                        
            if (transaction != null)
            {
                PublishMessage(message, destination, forceDestination, publish, transaction, name);
            }
            else
            {
                _elasticApmtracer.CaptureTransaction(name, TransactionType, () => Publish(message, destination, forceDestination, publish));
            }
        }        

        public async Task<T> PublishAndWait<T>(IMessage message, string destination, bool forceDestination, int milliSecondsTimeout,
            Func<IMessage, string, bool, int, Task<T>> publishAndWait) where T : IMessage
        {
            var method = new StackFrame().GetMethod();
            var name = GetName(message, method, publishAndWait.GetMethodInfo().Name);
            var transaction = _elasticApmtracer.CurrentTransaction;             
            if (transaction != null)
            {
                return await PublishMessageAndWait(message, destination, forceDestination, milliSecondsTimeout, publishAndWait, transaction, name);
            }
            return await _elasticApmtracer.CaptureTransaction(name, TransactionType, async () => await PublishAndWait(message, destination, forceDestination, milliSecondsTimeout, publishAndWait));
        }
        
        
        private static string GetName(IMessage message, MemberInfo method, string actionName)
        {            
            var name = message.Headers.ContainsKey(ApmConstants.TransactionName)
                ? message.Headers[ApmConstants.TransactionName].ToString()
                : $"{method.DeclaringType}.{method.Name}.{actionName}";
            return name;
        }        

        private void PublishMessage(IMessage message, string destination, bool forceDestination, Action<IMessage, string, bool> publish,
            IExecutionSegment transaction, string name)
        {
            var span = transaction.StartSpan(name, SpanType);
            try
            {
                var traceParent = $"00-{transaction.TraceId}-{span.Id}-01";
                var traceAsyncTransaction = _config.GetValue<bool>(EnvironmentConstants.ApmTraceAsyncTransaction);
                message.Headers[ApmConstants.TraceParent] = traceParent;
                message.Headers[ApmConstants.ApmTraceAsyncTransaction] = traceAsyncTransaction;
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
                message.Headers[ApmConstants.TraceParent] = traceParent;
                message.Headers[ApmConstants.ApmTraceAsyncTransaction] = true;
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