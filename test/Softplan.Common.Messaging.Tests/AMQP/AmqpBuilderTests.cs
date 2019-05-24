using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Softplan.Common.Messaging.AMQP;
using Softplan.Common.Messaging.Infrastructure;
using Softplan.Common.Messaging.Tests.Properties;
using Xunit;

namespace Softplan.Common.Messaging.Tests.AMQP
{
    public class AmqpBuilderTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IConnectionFactory> _connectionFactoryMock;
        private readonly Mock<IConnection> _connectionMock;
        private readonly Mock<IConfiguration> _settingsMock;
        private readonly Mock<ILogger<AmqpBuilder>> _loggerMock;
        private Mock<IModel> _channelMock;
        private AmqpBuilder _builder;
        
        private const string RabbitUrlKey = "RABBIT_URL";
        private const string RabbitUrlValue = "amqp://localhost";
        private const string RabbitApiUrlKey = "RABBIT_API_URL";
        private const string RabbitApiUrlValue = "http://localhost";
        private const string LoggerName = "Softplan.Common.Messaging.AMQP.AmqpBuilder";
        
        public AmqpBuilderTests()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            
            _loggerMock = new Mock<ILogger<AmqpBuilder>>(mockBehavior);
            _loggerMock.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            
            _loggerFactoryMock = new Mock<ILoggerFactory>(mockBehavior);
            _loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>()))
                 .Returns(_loggerMock.Object);
            
            _channelMock = new Mock<IModel>(mockBehavior);
            
            _connectionMock = new Mock<IConnection>(mockBehavior);
            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);
            
            _connectionFactoryMock = new Mock<IConnectionFactory>(mockBehavior);
            _connectionFactoryMock.Setup(c => c.CreateConnection())
                .Returns(_connectionMock.Object);

            _settingsMock = new Mock<IConfiguration>(mockBehavior);
            
            _settingsMock.Setup(s => s.GetSection(RabbitUrlKey))
                .Returns(GetMockConfigSection(RabbitUrlValue));
            _settingsMock.Setup(s => s.GetSection(RabbitApiUrlKey))
                .Returns(GetMockConfigSection(RabbitApiUrlValue));
            _builder = new AmqpBuilder(_settingsMock.Object, _loggerFactoryMock.Object, _connectionFactoryMock.Object);
        }
        
        
        [Fact]
        public void When_Create_Builder_Without_Logger_Factory_Should_Throw_Expected_exception()
        {
            Action action = () => new AmqpBuilder(_settingsMock.Object, null, _connectionFactoryMock.Object);
            
            action.Should()
                .Throw<ArgumentNullException>()
                .WithMessage(string.Format(Resources.ValueCannotBeNull, "factory"));
        }
        
        [Fact]
        public void When_Create_Builder_With_Logger_Factory_Should_Create_Logger()
        {
            _loggerFactoryMock.Invocations.Clear();
            var amqpBuilder = new AmqpBuilder(_settingsMock.Object, _loggerFactoryMock.Object, _connectionFactoryMock.Object);

            _loggerFactoryMock.Verify(l => l.CreateLogger(LoggerName), Times.Once);
        }
        
        [Fact]
        public void When_Create_Builder_Should_Create_Connection()
        {
            _connectionFactoryMock.Invocations.Clear();
            var amqpBuilder = new AmqpBuilder(_settingsMock.Object, _loggerFactoryMock.Object, _connectionFactoryMock.Object);

            _connectionFactoryMock.Verify(c => c.CreateConnection(), Times.Once);
        }
        
        [Fact]
        public void When_Create_Builder_Without_Connection_Factory_Should_Get_Configuration_Value()
        {
            try
            {
                var amqpBuilder = new AmqpBuilder(_settingsMock.Object, _loggerFactoryMock.Object);
            }
            finally
            {
                _settingsMock.Verify(s => s.GetSection(RabbitUrlKey), Times.Once);
            }
        }
        
        
        [Fact]
        public void When_BuildApiManager_Should_Return_Expected_Type()
        {
            var manager = _builder.BuildApiManager();
            
            manager.Should().BeOfType<RabbitMqApiManager>();                        
        }
        
        [Fact]
        public void When_BuildApiManager_Should_Log_Manager_Creation()
        {
            var manager = _builder.BuildApiManager();
            
            _loggerMock.Verify(l => l.Log(LogLevel.Trace, It.IsAny<EventId>(), new FormattedLogValues(Messaging.Properties.Resources.APIManagerCreating), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        

        [Fact]
        public void BuildApiManagerTest()
        {
            var manager = _builder.BuildApiManager();
            var manager2 = _builder.BuildApiManager();
            _connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            Assert.Equal(manager, manager2);
            _connectionMock.Verify(c => c.CreateModel(), Times.Once());
            _connectionMock.VerifyNoOtherCalls();
        }                
        
        
        [Fact]
        public void BuildConsumerTest()
        {
            _connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            var consumer = _builder.BuildConsumer();

            _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            _connectionMock.VerifyNoOtherCalls();
            Assert.IsType<AmqpConsumer>(consumer);
        }
        
        [Fact]
        public void BuildMessageTest()
        {
            _builder.MessageQueueMap["testQueue"] = typeof(Message);
            var message = _builder.BuildMessage("testQueue", 1, "{\"userId\": \"testUser\"}");
            Assert.IsType<Message>(message);
            Assert.Equal("testUser", message.UserId);
        }

        [Fact]
        public void BuildUnmappedMessage()
        {
            var err = Assert.Throws<KeyNotFoundException>(() => _builder.BuildMessage("unmappedQueue", 1));
            Assert.Equal("Não existe nenhuma mensagem mapeada para a fila unmappedQueue", err.Message);
        }
        

        [Fact]
        public void BuildPublisherTest()
        {
            _connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            var publisher = _builder.BuildPublisher();

            _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            _connectionMock.VerifyNoOtherCalls();
            Assert.IsType<AmqpPublisher>(publisher);
        }                
        
        
        private static IConfigurationSection GetMockConfigSection(string returnValue)
        {
            var res = new Mock<IConfigurationSection>();
            res.SetupGet(r => r.Value).Returns(returnValue);
            return res.Object;
        }
    }
}
