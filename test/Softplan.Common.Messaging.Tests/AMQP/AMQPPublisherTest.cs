using System;
using RabbitMQ.Client;
using Moq;
using Softplan.Common.Messaging.Abstractions;
using System.Collections.Generic;
using Softplan.Common.Messaging.Infrastructure;
using Softplan.Common.Messaging.AMQP;
using Xunit;
using System.Text;
using Moq.Protected;

namespace Softplan.Common.Messaging.UnitTest.AMQP
{
    public class AMQPPubliserTest : IDisposable
    {
        private readonly Mock<IModel> _channelMock;
        private readonly Mock<ISerializer> _serializerMock;
        private readonly Mock<IQueueApiManager> _managerMock;
        private readonly IPublisher _publisher;
        private IBasicProperties publishedMsgProps;

        const string testQueue = "testQueue";
        const string newTestQueue = "newTestQueue";
        const string replyToQueue = "replyToQueue";

        public AMQPPubliserTest()
        {
            var _properties = new Mock<IBasicProperties>();
            _properties.SetupProperty(p => p.Headers, new Dictionary<string, object>());
            _properties.SetupProperty(p => p.ReplyTo, null);

            _channelMock = new Mock<IModel>();
            _channelMock.Setup(chan => chan.BasicPublish(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
                .Callback<string, string, bool, IBasicProperties, byte[]>((exchange, routingKey, mandatory, props, body) => publishedMsgProps = props);
            _channelMock.Setup(chan => chan.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                    .Returns((string queueName, bool durable, bool exclusive, bool autodelete, IDictionary<string, object> props) => new QueueDeclareOk(queueName, 0, 0));
            _channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(_properties.Object);

            _serializerMock = new Mock<ISerializer>();
            _serializerMock.Setup(s => s.Serialize(It.IsAny<IMessage>(), It.IsAny<Encoding>()))
                .Returns(new byte[] { 1, 2, 3 });

            _managerMock = new Mock<IQueueApiManager>();
            _managerMock.Setup(m => m.GetQueueInfo(It.IsAny<string>()))
                .Returns(new QueueInfo());

            _publisher = new AmqpPublisher(_channelMock.Object, _serializerMock.Object, _managerMock.Object);
        }

        [Fact]
        public void SendToEmptyQueueNameTest()
        {
            var message = new Mock<IMessage>();

            var ex = Assert.Throws<ArgumentException>(() => _publisher.Publish(message.Object, ""));
            Assert.Equal("Destination cannot be empty.", ex.Message);
        }

        [Fact]
        public void SendMessageWithReplyQueueTest()
        {
            //given
            var message = new Mock<IMessage>();
            message.Setup(m => m.ReplyQueue).Returns(testQueue);
            message.Setup(m => m.Headers).Returns(new Dictionary<string, object>());
            //when
            _publisher.Publish(message.Object);
            //then
            _channelMock.Verify(chan => chan.BasicPublish(String.Empty, testQueue, false, It.IsAny<IBasicProperties>(), new byte[] { 1, 2, 3 }));
            _managerMock.Verify(m => m.EnsureQueue(testQueue));
            _channelMock.Verify(chan => chan.CreateBasicProperties());
            _channelMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void SendMessageWithReplyToTest()
        {
            //given
            var message = new Mock<IMessage>();
            message.Setup(m => m.ReplyTo).Returns(replyToQueue);
            message.Setup(m => m.Headers).Returns(new Dictionary<string, object>());
            //when
            _publisher.Publish(message.Object, testQueue);
            //then
            _channelMock.Verify(chan => chan.BasicPublish(String.Empty, testQueue, false, It.IsAny<IBasicProperties>(), new byte[] { 1, 2, 3 }));
            _managerMock.Verify(m => m.EnsureQueue(testQueue));
            _channelMock.Verify(chan => chan.CreateBasicProperties());
            _channelMock.VerifyNoOtherCalls();
            Assert.Equal(replyToQueue, publishedMsgProps.ReplyTo);
        }

        [Fact]
        public void SendMessageWithForceDestinationTest()
        {
            var message = new Mock<IMessage>();
            message.Setup(m => m.ReplyQueue).Returns(testQueue);
            message.Setup(m => m.Headers).Returns(new Dictionary<string, object>());

            _publisher.Publish(message.Object, newTestQueue, true);
            _channelMock.Verify(chan => chan.BasicPublish(String.Empty, newTestQueue, false, It.IsAny<IBasicProperties>(), new byte[] { 1, 2, 3 }));
            _managerMock.Verify(m => m.EnsureQueue(newTestQueue));
            _channelMock.Verify(chan => chan.CreateBasicProperties());
            _channelMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void SendMessageWithHeadersTest()
        {
            var message = new Mock<IMessage>();
            message.Setup(m => m.Headers).Returns(new Dictionary<string, object>() { { "key", 1 }, { "key2", "value2" } });

            _publisher.Publish(message.Object, newTestQueue);
            _channelMock.Verify(chan => chan.BasicPublish("", newTestQueue, false, It.IsAny<IBasicProperties>(), new byte[] { 1, 2, 3 }));
            _managerMock.Verify(m => m.EnsureQueue(newTestQueue));
            _channelMock.Verify(chan => chan.CreateBasicProperties());
            _channelMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void PublishAndWaitTimeOutTest()
        {
            var message = new Mock<IMessage>();
            message.Setup(m => m.Headers).Returns(new Dictionary<string, object>());
            _channelMock.Setup(c => c.BasicConsume(It.IsAny<string>(), It.IsAny<Boolean>(), It.IsAny<string>(),
                    It.IsAny<Boolean>(), It.IsAny<Boolean>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()))
                .Returns("consumerTag");
            _channelMock.Setup(c => c.QueueDeclare(String.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("replyQueue", 0, 0));

            var ex = Assert.ThrowsAsync<TimeoutException>(async () => await _publisher.PublishAndWait<Message>(message.Object, newTestQueue, true, 10)).Result;
        }

        public void Dispose()
        {
            //_channelMock
            GC.SuppressFinalize(_channelMock);
            GC.SuppressFinalize(_publisher);
        }
    }
}
