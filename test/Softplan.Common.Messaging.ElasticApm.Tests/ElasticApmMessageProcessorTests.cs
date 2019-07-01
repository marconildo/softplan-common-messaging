using System;
using System.Collections.Generic;
using Elastic.Apm.Api;
using Moq;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Xunit;

namespace Softplan.Common.Messaging.ElasticApm.Tests
{
    public class ElasticApmMessageProcessorTests
    {
        private ElasticApmMessageProcessor _elasticApmMessageProcessor;
        private Mock<ITracer> _elasticApmTracerMock;
        private Mock<IProcessor> _processorMock;
        private Mock<IPublisher> _publisherMock;
        private const string TransactionName = "Teste Transaction";
        private const string TraceParent = "19A0F56C-56A8-4609-9184-B0250380E515";
        private const string ProcessMessageTransacion =
            "Softplan.Common.Messaging.ElasticApm.ElasticApmMessageProcessor.ProcessMessage.ProcessMessage";
        private const string HandleProcessErrorTransaction =
            "Softplan.Common.Messaging.ElasticApm.ElasticApmMessageProcessor.HandleProcessError.HandleProcessError";

        public static IEnumerable<object[]> MessageData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new Message
                    {
                        Headers = { [ApmConstants.TransactionName] = TransactionName, [ApmConstants.ApmTraceAsyncTransaction] = true, [ApmConstants.TraceParent] = TraceParent }
                    },
                    true, true
                },
                new object[]
                {
                    new Message
                    {
                        Headers = { [ApmConstants.ApmTraceAsyncTransaction] = true, [ApmConstants.TraceParent] = TraceParent }
                    },
                    false, true
                },
                new object[]
                {
                    new Message
                    {
                        Headers = { [ApmConstants.TransactionName] = TransactionName, [ApmConstants.TraceParent] = TraceParent }
                    },
                    true, false
                },
                new object[]
                {
                    new Message
                    {
                        Headers = { [ApmConstants.TransactionName] = TransactionName, [ApmConstants.ApmTraceAsyncTransaction] = false, [ApmConstants.TraceParent] = TraceParent }
                    },
                    true, false
                },
                new object[]
                {
                    new Message
                    {
                        Headers = { [ApmConstants.TransactionName] = TransactionName, [ApmConstants.ApmTraceAsyncTransaction] = true }
                    },
                    true, false
                }
            };
        }

        public ElasticApmMessageProcessorTests()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            SetElasticApmTracerMock(mockBehavior);
            SetProcessorMock(mockBehavior);
            _publisherMock = new Mock<IPublisher>();
            _elasticApmMessageProcessor = new ElasticApmMessageProcessor(_elasticApmTracerMock.Object);
        }

        [Theory]
        [MemberData(nameof(MessageData))]
        public void ProcessMessage_should_call_CaptureTransaction_as_expected(Message message, bool withTransactionName,
            bool withTrace)
        {
            var transactionName = withTransactionName ? TransactionName : ProcessMessageTransacion;
            var traceParent = withTrace ? DistributedTracingData.TryDeserializeFromString(TraceParent) : null;

            _elasticApmMessageProcessor.ProcessMessage(message, _publisherMock.Object, _processorMock.Object.ProcessMessage);

            _elasticApmTracerMock.Verify(e => e.CaptureTransaction(transactionName, It.IsAny<string>(), It.IsAny<Action>(), traceParent));
        }

        [Theory]
        [MemberData(nameof(MessageData))]
        public void HandleProcessError_should_call_CaptureTransaction_as_expected(Message message,
            bool withTransactionName, bool withTrace)
        {
            var transactionName = withTransactionName ? TransactionName : HandleProcessErrorTransaction;
            var traceParent = withTrace ? DistributedTracingData.TryDeserializeFromString(TraceParent) : null;

            _elasticApmMessageProcessor.HandleProcessError(message, _publisherMock.Object, new Exception(), _processorMock.Object.HandleProcessError);

            _elasticApmTracerMock.Verify(e => e.CaptureTransaction(transactionName, It.IsAny<string>(), It.IsAny<Func<bool>>(), traceParent));
        }

        private void SetElasticApmTracerMock(MockBehavior mockBehavior)
        {
            _elasticApmTracerMock = new Mock<ITracer>(mockBehavior);
            _elasticApmTracerMock.Setup(e => e.CaptureTransaction(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<DistributedTracingData>()));
            _elasticApmTracerMock.Setup(e => e.CaptureTransaction(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<bool>>(), It.IsAny<DistributedTracingData>())).Returns(true);
        }

        private void SetProcessorMock(MockBehavior mockBehavior)
        {
            _processorMock = new Mock<IProcessor>(mockBehavior);
            _processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()));
            _processorMock.Setup(p => p.HandleProcessError(It.IsAny<IMessage>(), It.IsAny<IPublisher>(), It.IsAny<Exception>())).Returns(true);
        }
    }
}