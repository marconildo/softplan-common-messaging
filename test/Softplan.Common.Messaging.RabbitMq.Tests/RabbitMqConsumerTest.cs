using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.RabbitMq.Properties;
using Xunit;

namespace Softplan.Common.Messaging.RabbitMq.Tests
{
    public class RabbitMqConsumerTest
    {
        private class ProtectedConsumer : RabbitMqConsumer
        {
            public ProtectedConsumer(IModel channel, IPublisher publisher, IBuilder builder, IQueueApiManager manager) : base(channel, publisher, builder, manager)
            {

            }

            public void ProtectedOnMessageReceived(IProcessor processor, string queue, BasicDeliverEventArgs args)
            {
                OnMessageReceived(processor, queue, args);
            }
        }
        
        private Mock<IModel> _channelMock;
        private Mock<IBuilder> _builderMock;
        private Mock<IPublisher> _publisherMock;
        private Mock<IQueueApiManager> _managerMock;
        private Mock<IProcessor> _processorMock;
        private Mock<IBasicProperties> _basicPropertiesMock;
        private Mock<IMessage> _messageMock;
        private readonly BasicDeliverEventArgs _basicDeliverEventArgs;
        private readonly RabbitMqConsumer _consumer;
        
        private const string QueueName = "testQueue";
        private const string ConsumerTag = "consumerTag";
        private const string ReplyQueue = "replyQueue";
        private const string UserJson = "{\"userId\": \"123\"}";
        private const string Channel = "channel";
        private const string Publisher = "publisher";
        private const string Builder = "builder";
        private const string Manager = "manager";

        public RabbitMqConsumerTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            SetupChannelMock(mockBehavior);
            SetupMessageMock(mockBehavior);           
            SetupBuilderMock(mockBehavior);
            SetupManagerMock(mockBehavior);
            SetupProcessorMock(mockBehavior);
            SetupPublisherMock(mockBehavior);
            _consumer = new RabbitMqConsumer(_channelMock.Object, _publisherMock.Object, _builderMock.Object, _managerMock.Object);              
            SetupBasicPropertiesMock(mockBehavior);
            var userData = Encoding.UTF8.GetBytes(UserJson);
            _basicDeliverEventArgs = new BasicDeliverEventArgs(ConsumerTag, 1, false, string.Empty, QueueName, _basicPropertiesMock.Object, userData);
        }
        

        [Fact]
        public void When_Create_Consumer_Without_Channel_Should_Return_Expected_Exception()
        {
            TestAmqpConsumerConstructor(null, _publisherMock.Object, _builderMock.Object, _managerMock.Object, Channel);
        }

        [Fact]
        public void When_Create_Consumer_Without_Publisher_Should_Return_Expected_Exception()
        {
            TestAmqpConsumerConstructor(_channelMock.Object, null, _builderMock.Object, _managerMock.Object, Publisher);
        }

        [Fact]
        public void When_Create_Consumer_Without_Builder_Should_Return_Expected_Exception()
        {
            TestAmqpConsumerConstructor(_channelMock.Object, _publisherMock.Object, null, _managerMock.Object, Builder);
        }

        [Fact]
        public void When_Create_Consumer_Without_Manager_Should_Return_Expected_Exception()
        {
            TestAmqpConsumerConstructor(_channelMock.Object, _publisherMock.Object, _builderMock.Object, null, Manager);
        }        
        
        [Fact]
        public void When_Create_Consumer_Should_Set_Empty_ConsumerTag()
        {
            var consumer = new RabbitMqConsumer(_channelMock.Object,_publisherMock.Object, _builderMock.Object, _managerMock.Object);

            consumer.ConsumerTag.Should().BeEmpty();
        }
        
        
        [Fact]
        public void When_Start_Consumer_Already_Started_Should_Return_Expected_Exception()
        {            
            _consumer.Start(_processorMock.Object, QueueName);
            
            Action action = () => _consumer.Start(_processorMock.Object, QueueName);
            
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(Resources.ConsumerAlreadyStarted);
        }
        
        [Fact]
        public void When_Start_Consumer_Without_Queue_Should_Return_Expected_Exception()
        {
            Action action = () => _consumer.Start(_processorMock.Object, string.Empty);
            
            action.Should()
                .Throw<ArgumentNullException>()
                .WithMessage($"{Resources.QueueNameNotFound}\nParameter name: queue");
        } 
        
        [Fact]
        public void When_Start_Consumer_Should_Ensure_Queue()
        {
            _consumer.Start(_processorMock.Object, QueueName);
            
            _managerMock.Verify(m => m.EnsureQueue(QueueName), Times.Once);
        } 
        
        [Fact]
        public void When_Start_Consumer_Should_Call_BasicQos()
        {
            _consumer.Start(_processorMock.Object, QueueName);
            
            _channelMock.Verify(c => c.BasicQos(0, 1, false), Times.Once);
        }
        
        [Fact]
        public void When_Start_Consumer_Should_Call_BasicConsume()
        {
            _consumer.Start(_processorMock.Object, QueueName);
            
            _channelMock.Verify(c => c.BasicConsume(QueueName, false, It.IsAny<string>(),It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()));
        }
        
        [Fact]
        public void When_Start_Consumer_Should_Set_ConsumerTag()
        {
            _consumer.Start(_processorMock.Object, QueueName);

            _consumer.ConsumerTag.Should().Be(ConsumerTag);
        }                
                

        [Fact]
        public void When_Consume_Message_Should_Build_Message()
        {                                    
            ConsumeMessage();
            var data = Encoding.UTF8.GetString(_basicDeliverEventArgs.Body);
            
            _builderMock.Verify(b => b.BuildMessage(QueueName, 1, data), Times.Once);            
        }        

        [Theory]
        [InlineData(ReplyQueue, 1)]
        [InlineData("", 0)]
        public void When_Consume_Message_With_Reply_Should_Set_Reply_Queue(string replyTo, int times)
        {
            _basicPropertiesMock.SetupGet(p => p.ReplyTo).Returns(replyTo);
            _messageMock.SetupSet(m => m.ReplyQueue).Throws(new Exception());
            
            ConsumeMessage();
            _messageMock.VerifySet(m => m.ReplyQueue, Times.Exactly(times));                        
        }
        
        [Fact]
        public void When_Consume_Message_Should_Proccess_Message()
        {                                    
            ConsumeMessage();
            
            _processorMock.Verify(p => p.ProcessMessage(_messageMock.Object, _publisherMock.Object), Times.Once);          
        }
        
        [Fact]
        public void When_Consume_Message_With_Success_Should_Send_Ack_As_Expected()
        {                                    
            ConsumeMessage();
            
            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);       
        }
        
        [Fact]
        public void When_Consume_Message_With_Success_Should_Send_Nack_As_Expected()
        {                                    
            ConsumeMessage();
            
            _channelMock.Verify(c => c.BasicNack(1, false, true), Times.Never);      
        }
                                

        [Fact]
        public void When_Consume_Message_With_Error_And_Handle_Proccess_Error_Should_Send_Ack_As_Expected()
        {
            SetupToConsumeMessageWithErrorAndHandleProccess();

            ConsumeMessage();

            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
        }
        
        [Fact]
        public void When_Consume_Message_With_Error_And_Handle_Proccess_Error_Should_Send_Nack_As_Expected()
        {
            SetupToConsumeMessageWithErrorAndHandleProccess();

            ConsumeMessage();

            _channelMock.Verify(c => c.BasicNack(1, false, true), Times.Never);
        }        

        [Fact]
        public void When_Consume_Message_With_Error_And_Not_Handle_Proccess_Error_Should_Send_Ack_As_Expected()
        {
            SetupToConsumeMessageWithErrorAndNotHandleProccess();

            ConsumeMessage();

            _channelMock.Verify(c => c.BasicAck(1, false), Times.Never);
        }
        
        [Fact]
        public void When_Consume_Message_With_Error_And_Not_Handle_Proccess_Error_Should_Send_Nack_As_Expected()
        {
            SetupToConsumeMessageWithErrorAndNotHandleProccess();

            ConsumeMessage();

            _channelMock.Verify(c => c.BasicNack(1, false, true), Times.Once);
        }

        [Fact]
        public void When_Consume_Message_With_Error_And_Handle_Proccess_Error_Exception_Should_Send_Ack_As_Expected()
        {
            SetupToConsumeMessageWithErrorAndHandleProccessThrowsException();

            ConsumeMessage();

            _channelMock.Verify(c => c.BasicAck(1, false), Times.Never);
        }      
        
        [Fact]
        public void When_Consume_Message_With_Error_And_Handle_Proccess_Error_Exception_Should_Send_Nack_As_Expected()
        {
            SetupToConsumeMessageWithErrorAndHandleProccessThrowsException();

            ConsumeMessage();

            _channelMock.Verify(c => c.BasicNack(1, false, true), Times.Once);
        }
        
        
        [Fact]
        public void When_Stop_Started_Consumer_Should_Clear_ConsumerTag()
        {
            _consumer.Start(_processorMock.Object, QueueName);
            
            _consumer.Stop();

            _consumer.ConsumerTag.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Stop_Started_Consumer_Should_Call_BasicCancel()
        {
            _consumer.Start(_processorMock.Object, QueueName);
            
            _consumer.Stop();

            _channelMock.Verify(c => c.BasicCancel(ConsumerTag), Times.Once());
        }
        
        [Fact]
        public void When_Stop_Not_Started_Consumer_Should_Call_BasicCancel()
        {
            _consumer.Stop();

            _channelMock.Verify(c => c.BasicCancel(ConsumerTag), Times.Never);
        }
        
        
        private void SetupChannelMock(MockBehavior mockBehavior)
        {
            _channelMock = new Mock<IModel>(mockBehavior);
            _channelMock.Setup(c => c.BasicConsume(It.IsAny<string>(), false, It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()))
                .Returns(ConsumerTag);
            _channelMock.Setup(c => c.BasicCancel(ConsumerTag));
            _channelMock.Setup(c => c.BasicQos(It.IsAny<uint>(), It.IsAny<ushort>(), It.IsAny<bool>()));
            _channelMock.Setup(c => c.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));
            _channelMock.Setup(c => c.BasicNack(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        private void SetupMessageMock(MockBehavior mockBehavior)
        {
            _messageMock = new Mock<IMessage>(mockBehavior);
        }

        private void SetupBuilderMock(MockBehavior mockBehavior)
        {
            _builderMock = new Mock<IBuilder>(mockBehavior);
            _builderMock.Setup(b => b.BuildMessage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(_messageMock.Object);
        }

        private void SetupManagerMock(MockBehavior mockBehavior)
        {
            _managerMock = new Mock<IQueueApiManager>(mockBehavior);
            _managerMock.Setup(m => m.EnsureQueue(QueueName))
                .Returns(QueueName);
        }

        private void SetupProcessorMock(MockBehavior mockBehavior)
        {
            _processorMock = new Mock<IProcessor>(mockBehavior);
            _processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()));
        }

        private void SetupPublisherMock(MockBehavior mockBehavior)
        {
            _publisherMock = new Mock<IPublisher>(mockBehavior);
        }

        private void SetupBasicPropertiesMock(MockBehavior mockBehavior)
        {
            _basicPropertiesMock = new Mock<IBasicProperties>(mockBehavior);
            _basicPropertiesMock.SetupGet(p => p.ReplyTo).Returns(string.Empty);
        }
        
        private static void TestAmqpConsumerConstructor(IModel model, IPublisher publisher, IBuilder builder, IQueueApiManager manager, string parameter)
        {
            Action action = () => new RabbitMqConsumer(model, publisher, builder, manager);
            
            action.Should()
                .Throw<ArgumentNullException>()
                .WithMessage(string.Format(Properties.Resources.ValueCanNotBeNull, parameter));
        }
        
        private void ConsumeMessage()
        {
            var consumer = new ProtectedConsumer(_channelMock.Object, _publisherMock.Object, _builderMock.Object, _managerMock.Object);
            consumer.Start(_processorMock.Object, QueueName);
            consumer.ProtectedOnMessageReceived(_processorMock.Object, QueueName, _basicDeliverEventArgs);
            consumer.Stop();
        }
        
        private void SetupToConsumeMessageWithErrorAndHandleProccess()
        {
            _processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()))
                .Throws<Exception>();
            _processorMock.Setup(p => p.HandleProcessError(It.IsAny<IMessage>(), It.IsAny<IPublisher>(), It.IsAny<Exception>()))
                .Returns(true);
        }
        
        private void SetupToConsumeMessageWithErrorAndNotHandleProccess()
        {
            _processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()))
                .Throws<Exception>();
            _processorMock.Setup(p => p.HandleProcessError(It.IsAny<IMessage>(), It.IsAny<IPublisher>(), It.IsAny<Exception>()))
                .Returns(false);
        }
        
        private void SetupToConsumeMessageWithErrorAndHandleProccessThrowsException()
        {
            _processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()))
                .Throws<Exception>();
            _processorMock.Setup(p => p.HandleProcessError(It.IsAny<IMessage>(), It.IsAny<IPublisher>(), It.IsAny<Exception>()))
                .Throws<Exception>();
        }
    }
}
