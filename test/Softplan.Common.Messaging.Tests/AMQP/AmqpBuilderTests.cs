using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Softplan.Common.Messaging.AMQP;
using Softplan.Common.Messaging.Infrastructure;
using Xunit;

namespace Softplan.Common.Messaging.Tests.AMQP
{
    public class AmqpBuilderTests
    {
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<IConnectionFactory> connectionFactoryMock;
        private readonly Mock<IConnection> connectionMock;
        AmqpBuilder builder;

        private IConfigurationSection GetMockConfigSection(string returnValue)
        {
            var res = new Mock<IConfigurationSection>();
            res.SetupGet(r => r.Value).Returns(returnValue);
            return res.Object;
        }
        public AmqpBuilderTests()
        {
            loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>()))
                 .Returns(new Mock<ILogger<AmqpBuilder>>().Object);
            connectionMock = new Mock<IConnection>();
            connectionFactoryMock = new Mock<IConnectionFactory>();

            connectionFactoryMock.Setup(c => c.CreateConnection())
                .Returns(connectionMock.Object);

            var settings = new Mock<IConfiguration>();
            settings.Setup(s => s.GetSection("RABBIT_URL"))
                .Returns(GetMockConfigSection("amqp://localhost"));
            settings.Setup(s => s.GetSection("RABBIT_API_URL"))
                .Returns(GetMockConfigSection("http://locahost"));
            builder = new AmqpBuilder(settings.Object, loggerFactoryMock.Object, connectionFactoryMock.Object);
        }

        [Fact]
        public void BuildApiManagerTest()
        {
            var manager = builder.BuildApiManager();
            var manager2 = builder.BuildApiManager();
            connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            Assert.Equal(manager, manager2);
            connectionMock.Verify(c => c.CreateModel(), Times.Once());
            connectionMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void BuildPublisherTest()
        {
            connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            var publisher = builder.BuildPublisher();

            connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            connectionMock.VerifyNoOtherCalls();
            Assert.IsType<AmqpPublisher>(publisher);
        }

        [Fact]
        public void CreateBuilderWithUserAndPasswordForApiTest()
        {
            var settings = new Mock<IConfiguration>();
            settings.Setup(s => s.GetSection("RABBIT_URL"))
                .Returns(GetMockConfigSection("amqp://localhost"));
            settings.Setup(s => s.GetSection("RABBIT_API_URL"))
                .Returns(GetMockConfigSection("http://guest:guest@locahost"));

            builder = new AmqpBuilder(settings.Object, loggerFactoryMock.Object, connectionFactoryMock.Object);
            var manager = builder.BuildApiManager();
            Assert.IsType<RabbitMqApiManager>(manager);
        }

        [Fact]
        public void CreateBuilderWithoutConnectionFactoryTest()
        {
            var settings = new Mock<IConfiguration>();
            settings.Setup(s => s.GetSection("RABBIT_URL"))
                .Returns(GetMockConfigSection("amqp://invalidhost"));
            settings.Setup(s => s.GetSection("RABBIT_API_URL"))
                .Returns(GetMockConfigSection("http://guest:guest@locahost"));

            Assert.Throws<BrokerUnreachableException>(() => builder = new AmqpBuilder(settings.Object, loggerFactoryMock.Object, null));
        }

        [Fact]
        public void BuildConsumerTest()
        {
            connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            var consumer = builder.BuildConsumer();

            connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            connectionMock.VerifyNoOtherCalls();
            Assert.IsType<AmqpConsumer>(consumer);
        }

        [Fact]
        public void BuildMessageTest()
        {
            builder.MessageQueueMap["testQueue"] = typeof(Message);
            var message = builder.BuildMessage("testQueue", 1, "{\"userId\": \"testUser\"}");
            Assert.IsType<Message>(message);
            Assert.Equal("testUser", message.UserId);
        }

        [Fact]
        public void BuildUnmappedMessage()
        {
            var err = Assert.Throws<KeyNotFoundException>(() => builder.BuildMessage("unmappedQueue", 1));
            Assert.Equal("Não existe nenhuma mensagem mapeada para a fila unmappedQueue", err.Message);
        }
    }
}
