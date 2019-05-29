using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Properties;
using Xunit;

namespace Softplan.Common.Messaging.RabbitMq.Tests
{
    public class RabbitMqPublisherTest : IDisposable
    {
        private Mock<IModel> _channelMock;
        private Mock<IQueueApiManager> _managerMock;
        private Mock<ISerializer> _serializerMock;
        private readonly IPublisher _publisher;
        private IBasicProperties _publishedMsgProps;

        private const string TestQueue = "testQueue";
        private const string NewTestQueue = "newTestQueue";
        private const string ReplyToQueue = "replyToQueue";
        private const string ConsumerTag = "consumerTag";
        private const string ContentEncoding = "utf-8";

        public RabbitMqPublisherTest()
        {
            const MockBehavior behavior = MockBehavior.Strict;
            SetupChannelMock(behavior);
            SetupSerializerMock(behavior);
            SetupManagerMock(behavior);            
            _publisher = new RabbitMqPublisher(_channelMock.Object, _serializerMock.Object, _managerMock.Object);
        }
        
        
        [Fact]
        public void When_Publish_Should_Publish_To_Create_Basic_Properties()
        {
            var message = GetMessage();
            var headersExpected = new Dictionary<string, object>() {{"x-send-to-default-queue", "false"}};
            
            _publisher.Publish(message.Object, TestQueue);
            
            _channelMock.Verify(c => c.CreateBasicProperties(), Times.Once);      
            _publishedMsgProps.Headers.Should().BeEquivalentTo(headersExpected);
            _publishedMsgProps.Persistent.Should().BeTrue();
            _publishedMsgProps.DeliveryMode.Should().Be(2);
            _publishedMsgProps.ContentEncoding.Should().Be(ContentEncoding);
            _publishedMsgProps.ReplyTo.Should().BeNull();
        }
        
        [Fact]
        public void When_Publish_Should_Serialize_Message()
        {
            var message = GetMessage();
            
            _publisher.Publish(message.Object, TestQueue);
            
            _serializerMock.Verify(s => s.Serialize(It.IsAny<IMessage>(), It.IsAny<Encoding>()), Times.Once);       
        }
        
        [Fact]
        public void When_Publish_With_Destination_Without_ReplyQueue_Without_Force_Destination_Should_Publish_To_Destination()
        {
            var message = GetMessage();
            
            _publisher.Publish(message.Object, TestQueue);
            
            _channelMock.Verify(c => c.BasicPublish(string.Empty, TestQueue, false, It.IsAny<IBasicProperties>(), new byte[] { 1, 2, 3 }), Times.Once);        
        }
        
        [Fact]
        public void When_Publish_With_Destination_Without_ReplyQueue_Without_Force_Destination_Should_Ensure_Queue_To_Destination()
        {
            var message = GetMessage();
            
            _publisher.Publish(message.Object, TestQueue);
            
            _managerMock.Verify(m => m.EnsureQueue(TestQueue), Times.Once);        
        }
                
        [Fact]
        public void When_Publish_With_Destination_With_ReplyQueue_Without_Force_Destination_Should_Publish_To_Reply_Queue()
        {
            var message = GetMessage(ReplyToQueue);

            _publisher.Publish(message.Object, TestQueue);
            
            _channelMock.Verify(c => c.BasicPublish(string.Empty, ReplyToQueue, false, It.IsAny<IBasicProperties>(), new byte[] { 1, 2, 3 }), Times.Once);
        }    
        
        [Fact]
        public void When_Publish_With_Destination_With_ReplyQueue_Without_Force_Destination_Should_Ensure_Queue_To_Reply_Queue()
        {
            var message = GetMessage(ReplyToQueue);

            _publisher.Publish(message.Object, TestQueue);
            
            _managerMock.Verify(m => m.EnsureQueue(ReplyToQueue), Times.Once);
        }
        
        [Fact]
        public void When_Publish_With_Destination_With_ReplyQueue_With_Force_Destination_Should_Publish_To_Destination()
        {
            var message = GetMessage(ReplyToQueue);

            _publisher.Publish(message.Object, TestQueue, true);
            
            _channelMock.Verify(c => c.BasicPublish(string.Empty, TestQueue, false, It.IsAny<IBasicProperties>(), new byte[] { 1, 2, 3 }), Times.Once);
        }    
        
        [Fact]
        public void When_Publish_With_Destination_With_ReplyQueue_With_Force_Destination_Should_Ensure_Queue_To_Destination()
        {
            var message = GetMessage(ReplyToQueue);

            _publisher.Publish(message.Object, TestQueue, true);
            
            _managerMock.Verify(m => m.EnsureQueue(TestQueue), Times.Once);
        }

        [Fact]
        public void Whend_Publish_With_Reply_In_Message_Should_Set_Property_Reply_To()
        {
            var message = GetMessage(string.Empty, ReplyToQueue);
            
            _publisher.Publish(message.Object, TestQueue);

            _publishedMsgProps.ReplyTo.Should().Be(ReplyToQueue);
        }
        
        [Fact]
        public void Whend_Publish_With_Headers_Should_Set_Headers()
        {
            var headers = new Dictionary<string, object>() {{"key", 1}, {"key2", "value2"}};
            var headersExpected = new Dictionary<string, object>() {{"key", 1}, {"key2", "value2"}, {"x-send-to-default-queue", "false"}};
            var message = new Mock<IMessage>();
            message.Setup(m => m.Headers).Returns(headers);
            
            _publisher.Publish(message.Object, TestQueue);

            _publishedMsgProps.Headers.Should().BeEquivalentTo(headersExpected);
        }    
        
        [Fact]
        public void When_Publish_Without_Destination_Without_Reply_Queue_Should_Return_Expected_Exeption()
        {
            var message = new Mock<IMessage>();

            Action action = () => _publisher.Publish(message.Object);

            action.Should()
                .Throw<ArgumentException>()
                .WithMessage(Resources.MessageDestionationIsNull);
        }

        [Fact]
        public void When_PublishAndWait_Should_Declare_Queue()
        {
            var message = GetMessage();                       
                        
            _publisher.PublishAndWait<Message>(message.Object, NewTestQueue, true, 10);

            _channelMock.Verify(c => c.QueueDeclare(string.Empty, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
        }
        
        [Fact]
        public void When_PublishAndWait_Should_Call_BasicConsume()
        {            
            try
            {
                var message = GetMessage();
                _publisher.PublishAndWait<Message>(message.Object, NewTestQueue, true, 10);
            }
            catch (Exception e)
            {
                _channelMock.Verify(c => c.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(),It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()), Times.Once);
            }                        
        }
        
        [Fact]
        public void When_PublishAndWait_And_Not_Receive_Response_In_Time_Should_Return_TimeOut()
        {
            var message = GetMessage();                       
                        
            Func<Task> function = async () => await _publisher.PublishAndWait<Message>(message.Object, NewTestQueue, true, 10);

            function.Should()
                .Throw<TimeoutException>()
                .WithMessage(Resources.ReplyMessageNotReceived);            
        }
                

        public void Dispose()
        {
            //_channelMock
            GC.SuppressFinalize(_channelMock);
            GC.SuppressFinalize(_publisher);
        }
        
        private void SetupChannelMock(MockBehavior behavior)
        {
            var properties = GetProperties();

            _channelMock = new Mock<IModel>(behavior);
            _channelMock.Setup(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
                .Callback<string, string, bool, IBasicProperties, byte[]>((exchange, routingKey, mandatory, props, body) => _publishedMsgProps = props);
            _channelMock.Setup(c => c.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),It.IsAny<IDictionary<string, object>>()))
                .Returns((string queueName, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> props) => new QueueDeclareOk(queueName, 0, 0));
            _channelMock.Setup(c => c.CreateBasicProperties()).Returns(properties.Object);
            _channelMock.Setup(c => c.BasicCancel(It.IsAny<string>()));
            _channelMock.Setup(c => c.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(),It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()))
                .Returns(ConsumerTag);
        }

        private static Mock<IBasicProperties> GetProperties()
        {
            var properties = new Mock<IBasicProperties>();
            properties.SetupProperty(p => p.Headers, new Dictionary<string, object>());
            properties.SetupProperty(p => p.Persistent, false);
            properties.SetupProperty(p => p.DeliveryMode);
            properties.SetupProperty(p => p.ContentEncoding, null);
            properties.SetupProperty(p => p.ReplyTo, null);
            return properties;
        }

        private void SetupSerializerMock(MockBehavior behavior)
        {
            _serializerMock = new Mock<ISerializer>(behavior);
            _serializerMock.Setup(s => s.Serialize(It.IsAny<IMessage>(), It.IsAny<Encoding>())).Returns(new byte[] {1, 2, 3});                        
        }

        private void SetupManagerMock(MockBehavior behavior)
        {
            _managerMock = new Mock<IQueueApiManager>(behavior);
            _managerMock.Setup(m => m.GetQueueInfo(It.IsAny<string>())).Returns(new QueueInfo());    
            _managerMock.Setup(m => m.EnsureQueue(It.IsAny<string>())).Returns(NewTestQueue);
        }
        
        private static Mock<IMessage> GetMessage(string replyQueue = "", string replyToQueue = "")
        {
            var message = new Mock<IMessage>();
            message.Setup(m => m.ReplyQueue).Returns(replyQueue);
            message.Setup(m => m.ReplyTo).Returns(replyToQueue);
            message.Setup(m => m.Headers).Returns(new Dictionary<string, object>());
            return message;
        }
    }
}
