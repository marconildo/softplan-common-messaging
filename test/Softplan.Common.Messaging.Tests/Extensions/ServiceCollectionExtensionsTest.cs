using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Softplan.Common.Messaging.Extensions;
using Softplan.Common.Messaging.RabbitMq;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Extensions
{
    public class ServiceCollectionExtensionsTest
    {
        private readonly IServiceCollection _services;

        private Mock<IConfigurationSection> _configurationMessageBrokerUrlSectionMock;
        private Mock<IConfigurationSection> _configurationMessageBrokerApiUrlSectionMock;
        private Mock<IConfigurationSection> _configurationMessageBrokerSectionMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILoggerFactory> _loggerFactoryMock;

        private const string MessageBrokerUrl = "amqp://localhost";  
        private const string MessageBrokerApiUrl = "amqp://localhost";  
        private const string MessageBroker = "RabbitMq";  

        public ServiceCollectionExtensionsTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;            
            SetConfigurationMessageParameters(mockBehavior);
            SetConfigurationMock(mockBehavior);
            SetLoggerFactoryMock(mockBehavior);
            _services = new ServiceCollection();
        }        

        [Theory]
        [InlineData(typeof(IMessagingBuilderFactory))]
        [InlineData(typeof(IBuilder))]
        [InlineData(typeof(IPublisher))]
        [InlineData(typeof(IMessagingManager))]
        public void When_Call_AddMessagingManager_Should_Add_Expected_Services(Type type)
        {
            _services.AddMessagingManager(_configurationMock.Object, _loggerFactoryMock.Object);
            
            _services.Should().HaveCount(4);
            var types = _services.Select(s => s.ServiceType).ToArray();
            types.Should().Contain(type);
        }
        
        [Fact]
        public void When_Call_AddMessagingManager_Without_Configuration_Should_Not_Add_Services()
        {
            _configurationMessageBrokerUrlSectionMock.Setup(c => c.Value).Returns(string.Empty);
            _configurationMessageBrokerApiUrlSectionMock.Setup(c => c.Value).Returns(string.Empty);
            _configurationMessageBrokerSectionMock.Setup(c => c.Value).Returns((string) null);            
            
            _services.AddMessagingManager(_configurationMock.Object, _loggerFactoryMock.Object);
            
            _services.Should().HaveCount(0);
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
    }
}