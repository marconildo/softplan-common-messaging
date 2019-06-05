using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.ElasticApm;
using Xunit;

namespace Softplan.Common.Messaging.Tests
{
    public class MessagingWorkersFactoryTest
    {
        private Mock<IConfigurationSection> _configurationApmProviderSectionMock;
        private Mock<IConfiguration> _configurationMock;
        private readonly MessagingWorkersFactory _messagingWorkersFactory;
        
        private const string ApmProvider = "ElasticApm";
        
        public MessagingWorkersFactoryTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            _messagingWorkersFactory = new MessagingWorkersFactory();            
            SetConfigurationMessageParameters(mockBehavior);
            SetConfigurationMock(mockBehavior);
        }
                        
        
        [Fact]
        public void When_GetMessageProcessor_Whith_No_Configuration_Should_Return_DefaultMessageProcessor()
        {
            _configurationApmProviderSectionMock.Setup(c => c.Value).Returns((string) null);

            var builder = _messagingWorkersFactory.GetMessageProcessor(_configurationMock.Object);

            builder.Should().BeOfType<DefaultMessageProcessor>();
        }
        
        [Theory]
        [InlineData("ElasticApm", typeof(ElasticApmMessageProcessor))]
        public void When_GetMessageProcessor_Should_Return_Expected_According_Configuration(string messageBroker, Type expected)
        {
            _configurationApmProviderSectionMock.Setup(c => c.Value).Returns(messageBroker);
            
            var builder = _messagingWorkersFactory.GetMessageProcessor(_configurationMock.Object);

            builder.Should().BeOfType(expected);
        }
        
        
        [Fact]
        public void When_GetMessagePublisher_Whith_No_Configuration_Should_Return_DefaultMessagePublisher()
        {
            _configurationApmProviderSectionMock.Setup(c => c.Value).Returns((string) null);

            var builder = _messagingWorkersFactory.GetMessagePublisher(_configurationMock.Object);

            builder.Should().BeOfType<DefaultMessagePublisher>();
        }
        
        [Theory]
        [InlineData("ElasticApm", typeof(ElasticApmMessagePublisher))]
        public void When_GetMessagePublisher_Should_Return_Expected_According_Configuration(string messageBroker, Type expected)
        {
            _configurationApmProviderSectionMock.Setup(c => c.Value).Returns(messageBroker);
            
            var builder = _messagingWorkersFactory.GetMessagePublisher(_configurationMock.Object);

            builder.Should().BeOfType(expected);
        }
        
        
        private void SetConfigurationMessageParameters(MockBehavior mockBehavior)
        {
            _configurationApmProviderSectionMock = new Mock<IConfigurationSection>(mockBehavior);
            _configurationApmProviderSectionMock.Setup(c => c.Value).Returns(ApmProvider);
        }
        
        private void SetConfigurationMock(MockBehavior mockBehavior)
        {
            _configurationMock = new Mock<IConfiguration>(mockBehavior);
            _configurationMock.Setup(c => c.GetSection(EnvironmentConstants.ApmProvider))
                .Returns(_configurationApmProviderSectionMock.Object);
        }
    }
}