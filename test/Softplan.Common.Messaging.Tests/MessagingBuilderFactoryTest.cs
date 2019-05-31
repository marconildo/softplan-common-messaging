using System;
using System.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Softplan.Common.Messaging.Properties;
using Softplan.Common.Messaging.RabbitMq;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Constants;
using Xunit;

namespace Softplan.Common.Messaging.Tests
{
    public class MessagingBuilderFactoryTest
    {
        private readonly MessagingBuilderFactory _messagingBuilderFactory;
        
        private Mock<IConfigurationSection> _configurationMessageBrokerUrlSectionMock;
        private Mock<IConfigurationSection> _configurationMessageBrokerApiUrlSectionMock;
        private Mock<IConfigurationSection> _configurationMessageBrokerSectionMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILoggerFactory> _loggerFactoryMock;

        private const string MessageBrokerUrl = "amqp://localhost";  
        private const string MessageBrokerApiUrl = "amqp://localhost";  
        private const string MessageBroker = "RabbitMq";

        public MessagingBuilderFactoryTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            _messagingBuilderFactory = new MessagingBuilderFactory();            
            SetConfigurationMessageParameters(mockBehavior);
            SetConfigurationMock(mockBehavior);
            SetLoggerFactoryMock(mockBehavior);
        }

        [Fact]
        public void When_Has_Configuration_Should_Return_True()
        {
            var result = _messagingBuilderFactory.HasConfiguration(_configurationMock.Object);

            result.Should().BeTrue();
        }
        
        [Fact]
        public void When_Has_No_Configuration_Should_Return_False()
        {
            SetInvalidConfiguration();

            var result = _messagingBuilderFactory.HasConfiguration(_configurationMock.Object);

            result.Should().BeFalse();
        }

        
        [Fact]
        public void When_GetBuilder_Whith_No_Configuration_Should_Throws_The_Expected_Exception()
        {
            SetInvalidConfiguration();

            Action ex = () => _messagingBuilderFactory.GetBuilder(_configurationMock.Object, _loggerFactoryMock.Object);
            ex.Should().Throw<ConfigurationErrorsException>()
                .WithMessage(Resources.AmqpConfigurationNotFound);
        }

        [Theory]
        [InlineData("RabbitMq", typeof(RabbitMqBuilder))]
        public void When_GetBuilder_Should_Return_Expected_According_Configuration(string messageBroker, Type expected)
        {
            _configurationMessageBrokerSectionMock.Setup(c => c.Value).Returns(messageBroker);
            
            var builder = _messagingBuilderFactory.GetBuilder(_configurationMock.Object, _loggerFactoryMock.Object);

            builder.Should().BeOfType(expected);
        }
        
        
        private void SetConfigurationMessageParameters(MockBehavior mockBehavior)
        {
            _configurationMessageBrokerUrlSectionMock = new Mock<IConfigurationSection>(mockBehavior);
            _configurationMessageBrokerUrlSectionMock.Setup(c => c.Value).Returns(MessageBrokerUrl);
            _configurationMessageBrokerApiUrlSectionMock = new Mock<IConfigurationSection>(mockBehavior);
            _configurationMessageBrokerApiUrlSectionMock.Setup(c => c.Value).Returns(MessageBrokerApiUrl);
            _configurationMessageBrokerSectionMock = new Mock<IConfigurationSection>(mockBehavior);
            _configurationMessageBrokerSectionMock.Setup(c => c.Value).Returns(MessageBroker);
        }

        private void SetConfigurationMock(MockBehavior mockBehavior)
        {
            _configurationMock = new Mock<IConfiguration>(mockBehavior);
            _configurationMock.Setup(c => c.GetSection(EnvironmentConstants.MessageBrokerUrl))
                .Returns(_configurationMessageBrokerUrlSectionMock.Object);
            _configurationMock.Setup(c => c.GetSection(EnvironmentConstants.MessageBrokerApiUrl))
                .Returns(_configurationMessageBrokerApiUrlSectionMock.Object);
            _configurationMock.Setup(c => c.GetSection(EnvironmentConstants.MessageBroker))
                .Returns(_configurationMessageBrokerSectionMock.Object);
        }

        private void SetLoggerFactoryMock(MockBehavior mockBehavior)
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>(mockBehavior);
            _loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger<RabbitMqBuilder>>().Object);
        }
        
        private void SetInvalidConfiguration()
        {
            _configurationMessageBrokerUrlSectionMock.Setup(c => c.Value).Returns(string.Empty);
            _configurationMessageBrokerApiUrlSectionMock.Setup(c => c.Value).Returns(string.Empty);
            _configurationMessageBrokerSectionMock.Setup(c => c.Value).Returns((string) null);
        }
    }
}