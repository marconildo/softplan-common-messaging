using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Extensions;
using Softplan.Common.Messaging.RabbitMq;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Extensions
{
    public class ServiceCollectionExtensionsTest
    {
        private readonly IServiceCollection _services;

        private const string RabbitApiSection = "RABBIT_API_URL";
        private const string RabbitSection = "RABBIT_URL";
        private const string Url = "amqp://localhost";        

        public ServiceCollectionExtensionsTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            var configurationMock = new Mock<IConfiguration>(mockBehavior);
            var configurationSectionMock = new Mock<IConfigurationSection>(mockBehavior);
            var loggerFactoryMock = new Mock<ILoggerFactory>(mockBehavior);            
            var connectionFactoryMock = new Mock<IConnectionFactory>(mockBehavior);
            var connectionMock = new Mock<IConnection>(mockBehavior);            
            connectionFactoryMock.Setup(c => c.CreateConnection()).Returns(connectionMock.Object);            
            configurationMock.Setup(c => c.GetSection(RabbitSection)).Returns(configurationSectionMock.Object);
            configurationMock.Setup(c => c.GetSection(RabbitApiSection)).Returns(configurationSectionMock.Object);
            configurationSectionMock.Setup(c => c.Value).Returns(Url);
            loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger<RabbitMqBuilder>>().Object);
            connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            _services = new ServiceCollection();
            _services.AddMessagingManager(configurationMock.Object, loggerFactoryMock.Object, connectionFactoryMock.Object);
        }

        [Fact]
        public void When_Call_AddMessagingManager_Should_Add_Expected_Services()
        {
            _services.Should().HaveCount(3);
            _services[0].ServiceType.Should().Be(typeof(IBuilder));
            _services[1].ServiceType.Should().Be(typeof(IPublisher));
            _services[2].ServiceType.Should().Be(typeof(IMessagingManager));
        }
        
        [Theory]
        [InlineData(typeof(IBuilder), typeof(RabbitMqBuilder))]
        [InlineData(typeof(IPublisher), typeof(RabbitMqPublisher))]
        [InlineData(typeof(IMessagingManager), typeof(MessagingManager))]
        public void When_Call_AddMessagingManager_Should_Add_Expected_Service(Type interfaceType, Type classType)
        {            
            var service = _services.BuildServiceProvider().GetRequiredService(interfaceType);

            service.Should().BeOfType(classType);
        }
    }
}