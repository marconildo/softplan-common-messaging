using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using RabbitMQ.Client;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Tests.Properties;
using Xunit;

namespace Softplan.Common.Messaging.RabbitMq.Tests
{
    public class RabbitMqBuilderTest
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<IConnectionFactory> _connectionFactoryMock;
        private Mock<IConnection> _connectionMock;
        private Mock<IConfiguration> _settingsMock;
        private Mock<ILogger<RabbitMqBuilder>> _loggerMock;
        private RabbitMqBuilder _builder;
        
        private const string RabbitUrlKey = "RABBIT_URL";
        private const string RabbitUrlValue = "amqp://localhost";
        private const string RabbitApiUrlKey = "RABBIT_API_URL";
        private const string RabbitApiUrlValue = "http://user:password@localhost";
        private const string LoggerName = "Softplan.Common.Messaging.RabbitMq.RabbitMqBuilder";
        private const string TestQueueKey = "testQueue";
        private const string TestUserValue = "testUser";
        private const string UserData = "{\"userId\": \"testUser\"}";
        private const string UnmappedQueueKey = "unmappedQueue";
        
        public RabbitMqBuilderTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;           
            SetupLoggerMock(mockBehavior);
            SetupLoggerFactoryMock(mockBehavior);
            SetupConnectionMock(mockBehavior);
            SetupConnectionFactoryMock(mockBehavior);
            SetupSettingsMock(mockBehavior);
        }        


        [Fact]
        public void When_Create_Builder_Without_Logger_Factory_Should_Throw_Expected_exception()
        {
            Action action = () => new RabbitMqBuilder(_settingsMock.Object, null, _connectionFactoryMock.Object);
            
            action.Should()
                .Throw<ArgumentNullException>()
                .WithMessage(string.Format(Resources.ValueCanNotBeNull, "factory"));
        }
        
        [Fact]
        public void When_Create_Builder_With_Logger_Factory_Should_Create_Logger()
        {
            _loggerFactoryMock.Invocations.Clear();
            var amqpBuilder = new RabbitMqBuilder(_settingsMock.Object, _loggerFactoryMock.Object, _connectionFactoryMock.Object);

            _loggerFactoryMock.Verify(l => l.CreateLogger(LoggerName), Times.Once);
        }
        
        [Fact]
        public void When_Create_Builder_Should_Create_Connection()
        {
            _connectionFactoryMock.Invocations.Clear();
            var amqpBuilder = new RabbitMqBuilder(_settingsMock.Object, _loggerFactoryMock.Object, _connectionFactoryMock.Object);

            _connectionFactoryMock.Verify(c => c.CreateConnection(), Times.Once);
        }
        
        [Fact]
        public void When_Create_Builder_Without_Connection_Factory_Should_Get_RabbitUrlKey_Configuration_Value()
        {
            _settingsMock.Invocations.Clear();
            try
            {
                var amqpBuilder = new RabbitMqBuilder(_settingsMock.Object, _loggerFactoryMock.Object);
            }
            catch //Should throws exception because the new ConnectionFactory could not CreateConnection.
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
            
            _loggerMock.Verify(l => l.Log(LogLevel.Trace, It.IsAny<EventId>(), new FormattedLogValues(Messaging.RabbitMq.Properties.Resources.APIManagerCreating), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void When_BuildApiManager_Should_Get_RabbitApiUrlKey_Configuration_Value()
        {
            var manager = _builder.BuildApiManager();
            
            _settingsMock.Verify(s => s.GetSection(RabbitApiUrlKey), Times.Once);
        }
        
        [Fact]
        public void When_BuildApiManager_Should_Create_Model()
        {
            var manager = _builder.BuildApiManager();
            
            _connectionMock.Verify(c => c.CreateModel(), Times.Once);
        }
        
        [Fact]
        public void When_BuildApiManager_And_Manager_Already_Created_Should_Return_Same_Object()
        {
            var manager1 = _builder.BuildApiManager();
            var manager2 = _builder.BuildApiManager();

            manager1.Should().BeSameAs(manager2);
        }                
        
        
        [Fact]
        public void When_BuildConsumer_Should_Return_Expected_Type()
        {
            var consumer = _builder.BuildConsumer();

            consumer.Should().BeOfType<RabbitMqConsumer>();
        }
        
        [Fact]
        public void When_BuildConsumer_Should_Create_Model_As_Expected()
        {
            var consumer = _builder.BuildConsumer();

            _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            _connectionMock.VerifyNoOtherCalls();
        }
        
        
        [Fact]
        public void When_BuildMessage_Should_Return_Expected_Message()
        {            
            _builder.MessageQueueMap[TestQueueKey] = typeof(Message);
            
            var message = _builder.BuildMessage(TestQueueKey, 1, UserData);
            Assert.IsType<Message>(message);
            Assert.Equal(TestUserValue, message.UserId);
        }
        
        [Fact]
        public void When_Build_Unmapped_Message_Should_Throw_Expected_exception()
        {            
            Action action = () => _builder.BuildMessage(UnmappedQueueKey, 1);
            
            action.Should()
                .Throw<KeyNotFoundException>()
                .WithMessage(string.Format(Messaging.RabbitMq.Properties.Resources.NoMessagesMappedToQueue, UnmappedQueueKey));
        }
        

        [Fact]
        public void BuildPublisherTest()
        {
            _connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            var publisher = _builder.BuildPublisher();

            _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            _connectionMock.VerifyNoOtherCalls();
            Assert.IsType<RabbitMqPublisher>(publisher);
        }                
        
        
        private void SetupLoggerMock(MockBehavior mockBehavior)
        {
            _loggerMock = new Mock<ILogger<RabbitMqBuilder>>(mockBehavior);
            _loggerMock.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(),
                It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }

        private void SetupLoggerFactoryMock(MockBehavior mockBehavior)
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>(mockBehavior);
            _loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);
        }

        private void SetupConnectionMock(MockBehavior mockBehavior)
        {
            var channelMock = new Mock<IModel>(mockBehavior);
            _connectionMock = new Mock<IConnection>(mockBehavior);
            _connectionMock.Setup(c => c.CreateModel()).Returns(channelMock.Object);
        }

        private void SetupConnectionFactoryMock(MockBehavior mockBehavior)
        {
            _connectionFactoryMock = new Mock<IConnectionFactory>(mockBehavior);
            _connectionFactoryMock.Setup(c => c.CreateConnection())
                .Returns(_connectionMock.Object);
        }

        private void SetupSettingsMock(MockBehavior mockBehavior)
        {
            _settingsMock = new Mock<IConfiguration>(mockBehavior);
            _settingsMock.Setup(s => s.GetSection(RabbitUrlKey))
                .Returns(GetMockConfigSection(RabbitUrlValue));
            _settingsMock.Setup(s => s.GetSection(RabbitApiUrlKey))
                .Returns(GetMockConfigSection(RabbitApiUrlValue));
            _builder = new RabbitMqBuilder(_settingsMock.Object, _loggerFactoryMock.Object, _connectionFactoryMock.Object);
        }
        
        private static IConfigurationSection GetMockConfigSection(string returnValue)
        {
            var res = new Mock<IConfigurationSection>();
            res.SetupGet(r => r.Value).Returns(returnValue);
            return res.Object;
        }
    }
}
