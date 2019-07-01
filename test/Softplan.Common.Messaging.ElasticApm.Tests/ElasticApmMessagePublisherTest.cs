using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Xunit;

namespace Softplan.Common.Messaging.ElasticApm.Tests
{
    public class ElasticApmMessagePublisherTest
    {
        private ElasticApmMessagePublisher _elasticApmMessagePublisher;
        private Mock<IConfigurationSection> _configurationApmProviderSectionMock;
        private Mock<ITracer> _elasticApmTracerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ISpan> _spanMock;
        private Mock<ITransaction> _transactionMock;
        private Mock<IPublisher> _publisherMock;

        private const string SpanId = "19A0F56C";
        private const string TransactionName = "Teste Transaction";
        private const string TransactionId = "B0250380E515";
        private const string Destination = "Destination";
        private const string PublishTransacion =
            "Softplan.Common.Messaging.ElasticApm.ElasticApmMessagePublisher.Publish.Publish";
        private const string PublishAndWaitTransaction =
            "Softplan.Common.Messaging.ElasticApm.ElasticApmMessagePublisher+<PublishAndWait>d__8`1[T].MoveNext.PublishAndWait";

        public static IEnumerable<object[]> CurrentTransactionData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new Message { Headers = { [ApmConstants.TransactionName] = TransactionName }}
                }
            };
        }

        public static IEnumerable<object[]> CaptureTransactionData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new Message { Headers = {[ApmConstants.TransactionName] = TransactionName} },
                    true, true, Times.Never()
                },
                new object[]
                {
                    new Message { Headers = {[ApmConstants.TransactionName] = TransactionName} },
                    false, true, Times.Once()
                },
                new object[]
                {
                    new Message(),
                    true, false, Times.Never()
                },
                new object[]
                {
                    new Message(),
                    false, false, Times.Once()
                }
            };
        }
        
        public static IEnumerable<object[]> StartSpanData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new Message { Headers = {[ApmConstants.TransactionName] = TransactionName} },
                    true
                },
                new object[]
                {
                    new Message(),
                    false
                }
            };
        }
        
        public static IEnumerable<object[]> HeadersData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new Message { Headers = {[ApmConstants.TransactionName] = TransactionName} },
                    true
                },
                new object[]
                {
                    new Message { Headers = {[ApmConstants.TransactionName] = TransactionName} },
                    false
                }
            };
        }


        public ElasticApmMessagePublisherTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            SetConfigurationMessageParameters(mockBehavior);
            SetConfigurationMock(mockBehavior);
            SetSpanMock(mockBehavior);
            SetTransactionMock(mockBehavior);
            SetElasticApmTracerMock(mockBehavior);
            SetPublisherMock();
            _elasticApmMessagePublisher = new ElasticApmMessagePublisher(_configurationMock.Object, _elasticApmTracerMock.Object);
        }        


        [Theory]
        [MemberData(nameof(CurrentTransactionData))]
        public void Publish_should_verify_current_transaction_as_expected(IMessage message)
        {
            _elasticApmMessagePublisher.Publish(message, Destination, false, _publisherMock.Object.Publish);

            _elasticApmTracerMock.VerifyGet(e => e.CurrentTransaction);
        }

        [Theory]
        [MemberData(nameof(CaptureTransactionData))]
        public void Publish_should_start_new_transaction_when_necessary(IMessage message, bool activeTransaction, bool withTransactionName, Times times)
        {
            var transaction = activeTransaction ? _transactionMock.Object : null;
            _elasticApmTracerMock.SetupGet(e => e.CurrentTransaction).Returns(transaction);
            var transactionName = withTransactionName ? TransactionName : PublishTransacion;
            transactionName = activeTransaction ? It.IsAny<string>() : transactionName;
            
            _elasticApmMessagePublisher.Publish(message, Destination, false, _publisherMock.Object.Publish);

            _elasticApmTracerMock.Verify(e => e.CaptureTransaction(transactionName, It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<DistributedTracingData>()), times);
        }

        [Theory]
        [MemberData(nameof(StartSpanData))]
        public void Publish_should_start_span(IMessage message, bool withTransactionName)
        {
            var transactionName = withTransactionName ? TransactionName : PublishTransacion;
            
            _elasticApmMessagePublisher.Publish(message, Destination, false, _publisherMock.Object.Publish);

            _transactionMock.Verify(t => t.StartSpan(transactionName, It.IsAny<string>(), null, null), Times.Once);
        }
        
        [Theory]
        [MemberData(nameof(HeadersData))]
        public void Publish_should_set_expected_message_headers(IMessage message, bool apmTraceAsyncTransaction)
        {
            _configurationApmProviderSectionMock.Setup(c => c.Value).Returns(apmTraceAsyncTransaction.ToString);
            
            _elasticApmMessagePublisher.Publish(message, Destination, false, _publisherMock.Object.Publish);

            message.Headers[ApmConstants.TraceParent].Should().Be($"00-{TransactionId}-{SpanId}-01");
            message.Headers[ApmConstants.ApmTraceAsyncTransaction].Should().Be(apmTraceAsyncTransaction);
        }
        
        [Theory]
        [MemberData(nameof(CurrentTransactionData))]
        public void Publish_should_publish_message(IMessage message)
        {
            _elasticApmMessagePublisher.Publish(message, Destination, false, _publisherMock.Object.Publish);

            _publisherMock.Verify(p => p.Publish(message, Destination, false), Times.Once);
        }
        
        [Theory]
        [MemberData(nameof(CurrentTransactionData))]
        public void Publish_should_finalize_span(IMessage message)
        {
            _elasticApmMessagePublisher.Publish(message, Destination, false, _publisherMock.Object.Publish);

            _spanMock.Verify(t => t.End(), Times.Once);
        }
        
        [Fact]
        public void Publish_should_capture_exception()
        {
            _publisherMock.Setup(p => p.Publish(It.IsAny<IMessage>(), It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());
            var message = new Message
            {
                Headers = {[ApmConstants.TransactionName] = TransactionName}
            };
            Action action = () =>  _elasticApmMessagePublisher.Publish(message, Destination, false, _publisherMock.Object.Publish);
            
            action.Should().Throw<Exception>();            
            _spanMock.Verify(t => t.CaptureException(It.IsAny<Exception>(),null, false, null), Times.Once);
        }
        
        
        [Theory]
        [MemberData(nameof(CurrentTransactionData))]
        public async void PublishAndWait_should_verify_current_transaction_as_expected(IMessage message)
        {
            var messageResponse = await _elasticApmMessagePublisher.PublishAndWait(message, Destination, false, 5, _publisherMock.Object.PublishAndWait<IMessage>);

            _elasticApmTracerMock.VerifyGet(e => e.CurrentTransaction);
        }
        
        [Theory]
        [MemberData(nameof(CaptureTransactionData))]
        public async void PublishAndWait_should_start_new_transaction_when_necessary(IMessage message, bool activeTransaction, bool withTransactionName, Times times)
        {
            var transaction = activeTransaction ? _transactionMock.Object : null;
            _elasticApmTracerMock.SetupGet(e => e.CurrentTransaction).Returns(transaction);
            var transactionName = withTransactionName ? TransactionName : PublishAndWaitTransaction;
            transactionName = activeTransaction ? It.IsAny<string>() : transactionName;
            
            var messageResponse = await _elasticApmMessagePublisher.PublishAndWait(message, Destination, false, 5, _publisherMock.Object.PublishAndWait<IMessage>);

            _elasticApmTracerMock.Verify(e => e.CaptureTransaction(transactionName, It.IsAny<string>(), It.IsAny<Func<Task<IMessage>>>(), It.IsAny<DistributedTracingData>()), times);
        }
        
        [Theory]
        [MemberData(nameof(StartSpanData))]
        public async void PublishAndWait_should_start_span(IMessage message, bool withTransactionName)
        {
            var transactionName = withTransactionName ? TransactionName : PublishAndWaitTransaction;
            
            var messageResponse = await _elasticApmMessagePublisher.PublishAndWait(message, Destination, false, 5, _publisherMock.Object.PublishAndWait<IMessage>);

            _transactionMock.Verify(t => t.StartSpan(transactionName, It.IsAny<string>(), null, null), Times.Once);
        }
        
        [Theory]
        [MemberData(nameof(CurrentTransactionData))]
        public async void PublishAndWait_should_set_expected_message_headers(IMessage message)
        {
            var messageResponse = await _elasticApmMessagePublisher.PublishAndWait(message, Destination, false, 5, _publisherMock.Object.PublishAndWait<IMessage>);

            message.Headers[ApmConstants.TraceParent].Should().Be($"00-{TransactionId}-{SpanId}-01");
            message.Headers[ApmConstants.ApmTraceAsyncTransaction].Should().Be(true);
        }
        
        [Theory]
        [MemberData(nameof(CurrentTransactionData))]
        public async void PublishAndWait_should_publish_message(IMessage message)
        {
            var messageResponse = await _elasticApmMessagePublisher.PublishAndWait(message, Destination, false, 5, _publisherMock.Object.PublishAndWait<IMessage>);

            _publisherMock.Verify(p => p.PublishAndWait<IMessage>(It.IsAny<IMessage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
        }
        
        [Theory]
        [MemberData(nameof(CurrentTransactionData))]
        public async void PublishAndWait_should_finalize_span(IMessage message)
        {
            var messageResponse = await _elasticApmMessagePublisher.PublishAndWait(message, Destination, false, 5, _publisherMock.Object.PublishAndWait<IMessage>);

            _spanMock.Verify(t => t.End(), Times.Once);
        }
        
        [Fact]
        public void PublishAndWait_should_capture_exception()
        {
            _publisherMock.Setup(p => p.PublishAndWait<IMessage>(It.IsAny<IMessage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).Throws(new Exception());
            var message = new Message
            {
                Headers = {[ApmConstants.TransactionName] = TransactionName}
            };
            
            Func<IMessage> func = () => _elasticApmMessagePublisher.PublishAndWait(message, Destination, false, 5, _publisherMock.Object.PublishAndWait<IMessage>).Result;
            
            func.Should().Throw<Exception>();            
            _spanMock.Verify(t => t.CaptureException(It.IsAny<Exception>(),null, false, null), Times.Once);
        }


        private void SetConfigurationMessageParameters(MockBehavior mockBehavior)
        {
            _configurationApmProviderSectionMock = new Mock<IConfigurationSection>(mockBehavior);
            _configurationApmProviderSectionMock.Setup(c => c.Value).Returns("true");
        }

        private void SetConfigurationMock(MockBehavior mockBehavior)
        {
            _configurationMock = new Mock<IConfiguration>(mockBehavior);
            _configurationMock.Setup(c => c.GetSection(EnvironmentConstants.ApmTraceAsyncTransaction)).Returns(_configurationApmProviderSectionMock.Object);
        }

        private void SetSpanMock(MockBehavior mockBehavior)
        {
            _spanMock = new Mock<ISpan>(mockBehavior);
            _spanMock.SetupGet(s => s.Id).Returns(SpanId);
            _spanMock.Setup(s => s.CaptureException(It.IsAny<Exception>(), null, false, null));
            _spanMock.Setup(s => s.End());
        }

        private void SetTransactionMock(MockBehavior mockBehavior)
        {
            _transactionMock = new Mock<ITransaction>(mockBehavior);
            _transactionMock.SetupGet(t => t.TraceId).Returns(TransactionId);
            _transactionMock.Setup(t => t.StartSpan(It.IsAny<string>(), It.IsAny<string>(), null, null)).Returns(_spanMock.Object);
        }

        private void SetElasticApmTracerMock(MockBehavior mockBehavior)
        {
            _elasticApmTracerMock = new Mock<ITracer>(mockBehavior);
            _elasticApmTracerMock.Setup(e => e.CaptureTransaction(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<DistributedTracingData>()));
            _elasticApmTracerMock.Setup(e => e.CaptureTransaction<IMessage>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<Task<IMessage>>>(), It.IsAny<DistributedTracingData>())).ReturnsAsync(new Message());
            _elasticApmTracerMock.SetupGet(e => e.CurrentTransaction).Returns(_transactionMock.Object);
        }
        
        private void SetPublisherMock()
        {
            _publisherMock = new Mock<IPublisher>();
            _publisherMock.Setup(p => p.Publish(It.IsAny<IMessage>(), It.IsAny<string>(), It.IsAny<bool>()));
            _publisherMock.Setup(p => p.PublishAndWait<IMessage>(It.IsAny<IMessage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new Message());
        }
    }
}