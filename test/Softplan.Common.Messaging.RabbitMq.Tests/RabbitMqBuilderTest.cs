using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.RabbitMq.Tests.Properties;
using Xunit;

namespace Softplan.Common.Messaging.RabbitMq.Tests
{
    public class RabbitMqBuilderTest
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<IConnectionFactory> _connectionFactoryMock;
        private Mock<IConnection> _connectionMock;
        private Mock<IConfiguration> _configMock;
        private Mock<ILogger<RabbitMqBuilder>> _loggerMock;
        private Mock<IMessageProcessor> _messageProcessorMock;
        private Mock<IMessageProcessorFactory> _messageProcessorFactoryMock;
        private RabbitMqBuilder _builder;
        
        private const string RabbitUrlValue = "amqp://localhost";
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
            SetupMessageProcessorFactoryMock(mockBehavior);                            
            _builder = new RabbitMqBuilder(_configMock.Object, _loggerFactoryMock.Object, _messageProcessorFactoryMock.Object)
            {
                ConnectionFactory = _connectionFactoryMock.Object
            };
        }        


        [Fact]
        public void When_Create_Builder_Without_Logger_Factory_Should_Throw_Expected_exception()
        {
            Action action = () => new RabbitMqBuilder(_configMock.Object, null, _messageProcessorFactoryMock.Object)
            {
                ConnectionFactory = _connectionFactoryMock.Object
            };
            
            action.Should()
                .Throw<ArgumentNullException>()
                .WithMessage(string.Format(Resources.ValueCanNotBeNull, "factory"));
        }
        
        [Fact]
        public void When_Create_Builder_With_Logger_Factory_Should_Create_Logger()
        {
            _loggerFactoryMock.Invocations.Clear();
            var amqpBuilder = new RabbitMqBuilder(_configMock.Object, _loggerFactoryMock.Object, _messageProcessorFactoryMock.Object)
            {
                ConnectionFactory = _connectionFactoryMock.Object
            };

            _loggerFactoryMock.Verify(l => l.CreateLogger(LoggerName), Times.Once);
        }                
        
        [Fact]
        public void When_Create_Builder_Without_Connection_Factory_Should_Get_RabbitUrlKey_Configuration_Value()
        {
            _configMock.Invocations.Clear();
            try
            {
                var amqpBuilder = new RabbitMqBuilder(_configMock.Object, _loggerFactoryMock.Object, _messageProcessorFactoryMock.Object);
            }
            catch //Should throws exception because the new ConnectionFactory could not CreateConnection.
            {
                _configMock.Verify(s => s.GetSection(EnvironmentConstants.MessageBrokerUrl), Times.Once);
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
            
            _configMock.Verify(s => s.GetSection(EnvironmentConstants.MessageBrokerApiUrl), Times.Once);
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
        public void When_BuildApiManager_And_There_Is_No_Connection_Should_Create_Connection()
        {
            _connectionFactoryMock.Invocations.Clear();
            var manager = _builder.BuildApiManager();

            _connectionFactoryMock.Verify(c => c.CreateConnection(), Times.Once);
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
            _connectionMock.Invocations.Clear();
            var consumer = _builder.BuildConsumer();

            _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            _connectionMock.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void When_BuildConsumer_And_There_Is_No_Connection_Should_Create_Connection()
        {
            _connectionFactoryMock.Invocations.Clear();
            var consumer = _builder.BuildConsumer();

            _connectionFactoryMock.Verify(c => c.CreateConnection(), Times.Once);
            _connectionFactoryMock.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void When_BuildConsumer_Should_Get_MessageProcessor()
        {
            _messageProcessorFactoryMock.Invocations.Clear();
            var consumer = _builder.BuildConsumer();

            _messageProcessorFactoryMock.Verify(c => c.GetMessageProcessor(_configMock.Object), Times.Once);
            _messageProcessorFactoryMock.VerifyNoOtherCalls();
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
        public void When_BuildPublisher_Should_Call_CreateModel_As_expected()
        {
            _connectionMock.Invocations.Clear();
            var publisher = _builder.BuildPublisher();

            _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
            _connectionMock.VerifyNoOtherCalls();
        }   
        
        [Fact]
        public void When_BuildPublisher_Should_Return_Expected_Type()
        {
            var publisher = _builder.BuildPublisher();

            publisher.Should().BeOfType<RabbitMqPublisher>();
        } 
        
        [Fact]
        public void When_BuildPublisher_And_There_Is_No_Connection_Should_Create_Connection()
        {
            _connectionFactoryMock.Invocations.Clear();
            var publisher = _builder.BuildPublisher();

            _connectionFactoryMock.Verify(c => c.CreateConnection(), Times.Once);
        }
        
        
        [Fact]
        public void When_BuildSerializer_Should_Return_Expected_Type()
        {
            var serializer = _builder.BuildSerializer();

            serializer.Should().BeOfType<MessageSerializer>();
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
            _configMock = new Mock<IConfiguration>(mockBehavior);
            _configMock.Setup(s => s.GetSection(EnvironmentConstants.MessageBrokerUrl))
                .Returns(GetMockConfigSection(RabbitUrlValue));
            _configMock.Setup(s => s.GetSection(EnvironmentConstants.MessageBrokerApiUrl))
                .Returns(GetMockConfigSection(RabbitApiUrlValue));            
        }
        
        private void SetupMessageProcessorFactoryMock(MockBehavior mockBehavior)
        {
            _messageProcessorMock = new Mock<IMessageProcessor>(mockBehavior);
            _messageProcessorFactoryMock = new Mock<IMessageProcessorFactory>(mockBehavior);
            _messageProcessorFactoryMock.Setup(m => m.GetMessageProcessor(_configMock.Object))
                .Returns(_messageProcessorMock.Object);
        }
        
        private static IConfigurationSection GetMockConfigSection(string returnValue)
        {
            var res = new Mock<IConfigurationSection>();
            res.SetupGet(r => r.Value).Returns(returnValue);
            return res.Object;
        }
    }
}
