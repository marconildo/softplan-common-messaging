using System;
using System.Collections.Generic;
using Moq;
using RabbitMQ.Client;
using Xunit;

using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.AMQP;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace Softplan.Common.Messaging.UnitTest.AMQP
{
    public class AmqpConsumerTest
    {
        private class ProtectedConsumer : AmqpConsumer
        {
            public ProtectedConsumer(IModel channel, IPublisher publisher, IBuilder builder, IQueueApiManager manager) : base(channel, publisher, builder, manager)
            {

            }

            public void ProtectedOnMessageReceived(IProcessor processor, string queue, BasicDeliverEventArgs args)
            {
                OnMessageReceived(processor, queue, args);
            }
        }
        private readonly Mock<IModel> channelMock;
        private readonly Mock<IBuilder> builderMock;
        private readonly Mock<IPublisher> publisherMock;
        private readonly Mock<IQueueApiManager> managerMock;
        private readonly Mock<IProcessor> processorMock;
        private const string queueName = "testQueue";

        public AmqpConsumerTest()
        {
            channelMock = new Mock<IModel>();
            builderMock = new Mock<IBuilder>();
            publisherMock = new Mock<IPublisher>();
            managerMock = new Mock<IQueueApiManager>();
            processorMock = new Mock<IProcessor>();

            channelMock.Setup(c => c.BasicConsume(It.IsAny<string>(), false, It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>())).Returns("consumerTag");
            channelMock.Setup(c => c.BasicCancel("consumerTag"));
        }

        [Fact]
        public void StartConsumerTest()
        {
            var consumer = new AmqpConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);

            Assert.True(String.IsNullOrEmpty(consumer.ConsumerTag));
            consumer.Start(processorMock.Object, queueName);
            Assert.False(String.IsNullOrEmpty(consumer.ConsumerTag));
            consumer.Stop();
            Assert.True(String.IsNullOrEmpty(consumer.ConsumerTag));

            channelMock.Verify(c => c.BasicConsume(queueName, false, It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>()));
            managerMock.Verify(m => m.EnsureQueue(queueName));
        }

        [Fact]
        public void StartConsumerWithoutQueueTest()
        {
            var consumer = new AmqpConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);
            Assert.Throws<ArgumentNullException>(() => consumer.Start(processorMock.Object, string.Empty));
        }

        [Fact]
        public void StartConsumerAlreadyStartedTest()
        {
            var consumer = new AmqpConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);
            consumer.Start(processorMock.Object, queueName);
            Assert.Throws<InvalidOperationException>(() => consumer.Start(processorMock.Object, queueName));
        }

        [Fact]
        public void CreateConsumerWithoutChannel()
        {
            var err = Assert.Throws<ArgumentNullException>(() => new AmqpConsumer(null,
                publisherMock.Object, builderMock.Object, managerMock.Object));
            Assert.Matches("^Value cannot be null.\r?\nParameter name: channel$", err.Message);
        }

        [Fact]
        public void CreateConsumerWithoutPublisher()
        {
            var err = Assert.Throws<ArgumentNullException>(() => new AmqpConsumer(channelMock.Object,
                null, builderMock.Object, managerMock.Object));
            Assert.Matches("^Value cannot be null.\r?\nParameter name: publisher$", err.Message);
        }

        [Fact]
        public void CreateConsumerWithoutBuilder()
        {
            var err = Assert.Throws<ArgumentNullException>(() => new AmqpConsumer(channelMock.Object,
                publisherMock.Object, null, managerMock.Object));

            Assert.Matches("^Value cannot be null.\r?\nParameter name: builder$", err.Message);
        }

        [Fact]
        public void CreateConsumerWithoutManager()
        {
            var err = Assert.Throws<ArgumentNullException>(() => new AmqpConsumer(channelMock.Object,
                publisherMock.Object, builderMock.Object, null));
            Assert.Matches("^Value cannot be null.\r?\nParameter name: manager$", err.Message);
        }

        [Fact]
        public void ConsumeSimpleMessageTest()
        {
            var eventArgs = new BasicDeliverEventArgs("cTag", 1, false, "", "queue",
                new Mock<IBasicProperties>().Object, Encoding.UTF8.GetBytes("{\"userId\": \"123\"}"));

            var consumer = new ProtectedConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);
            consumer.Start(processorMock.Object, queueName);
            consumer.ProtectedOnMessageReceived(processorMock.Object, "queue", eventArgs);
            consumer.Stop();

            channelMock.Verify(c => c.BasicAck(1, false));
            channelMock.Verify(c => c.BasicNack(1, false, true), Times.Never);
        }

        [Fact]
        public void ErrorProcessingMessageHandlingOkTest()
        {
            var eventArgs = new BasicDeliverEventArgs("cTag", 1, false, "", "queue",
                new Mock<IBasicProperties>().Object, Encoding.UTF8.GetBytes("{\"userId\": \"123\"}"));
            processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()))
                .Throws<Exception>();
            processorMock.Setup(p => p.HandleProcessError(It.IsAny<IMessage>(), It.IsAny<IPublisher>(),
                It.IsAny<Exception>())).Returns(true);


            var consumer = new ProtectedConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);
            consumer.Start(processorMock.Object, queueName);
            consumer.ProtectedOnMessageReceived(processorMock.Object, "queue", eventArgs);
            consumer.Stop();

            channelMock.Verify(c => c.BasicAck(1, false));
            channelMock.Verify(c => c.BasicNack(1, false, true), Times.Never);
        }

        [Fact]
        public void ErrorProcessingMessageHandlingFalseTest()
        {
            var eventArgs = new BasicDeliverEventArgs("cTag", 1, false, "", "queue",
                new Mock<IBasicProperties>().Object, Encoding.UTF8.GetBytes("{\"userId\": \"123\"}"));
            processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()))
                .Throws<Exception>();
            processorMock.Setup(p => p.HandleProcessError(It.IsAny<IMessage>(), It.IsAny<IPublisher>(),
                It.IsAny<Exception>())).Returns(false);


            var consumer = new ProtectedConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);
            consumer.Start(processorMock.Object, queueName);
            consumer.ProtectedOnMessageReceived(processorMock.Object, "queue", eventArgs);
            consumer.Stop();

            channelMock.Verify(c => c.BasicAck(1, false), Times.Never);
            channelMock.Verify(c => c.BasicNack(1, false, true));
        }

        [Fact]
        public void ErrorProcessingMessageHandlingExceptionTest()
        {
            var eventArgs = new BasicDeliverEventArgs("cTag", 1, false, "", "queue",
                new Mock<IBasicProperties>().Object, Encoding.UTF8.GetBytes("{\"userId\": \"123\"}"));
            processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()))
                .Throws<Exception>();
            processorMock.Setup(p => p.HandleProcessError(It.IsAny<IMessage>(), It.IsAny<IPublisher>(),
                It.IsAny<Exception>())).Throws<Exception>();


            var consumer = new ProtectedConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);
            consumer.Start(processorMock.Object, queueName);
            consumer.ProtectedOnMessageReceived(processorMock.Object, "queue", eventArgs);
            consumer.Stop();

            channelMock.Verify(c => c.BasicAck(1, false), Times.Never);
            channelMock.Verify(c => c.BasicNack(1, false, true));
        }


        [Fact]
        public void ProcessMessageWithReplyToTest()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            basicPropertiesMock.SetupGet(p => p.ReplyTo).Returns("replyQueue");
            var eventArgs = new BasicDeliverEventArgs("cTag", 1, false, "", "queue",
                basicPropertiesMock.Object, Encoding.UTF8.GetBytes("{\"userId\": \"123\"}"));
            builderMock.Setup(b => b.BuildMessage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new Mock<IMessage>().Object);


            processorMock.Setup(p => p.ProcessMessage(It.IsAny<IMessage>(), It.IsAny<IPublisher>()));


            var consumer = new ProtectedConsumer(channelMock.Object, publisherMock.Object,
                builderMock.Object, managerMock.Object);
            consumer.Start(processorMock.Object, queueName);
            consumer.ProtectedOnMessageReceived(processorMock.Object, "queue", eventArgs);
            consumer.Stop();

            channelMock.Verify(c => c.BasicAck(1, false));
            channelMock.Verify(c => c.BasicNack(1, false, true), Times.Never);
        }
    }
}
