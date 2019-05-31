using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.Extensions;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Constants;
using Softplan.Common.Messaging.Tests.Properties;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Extensions
{
    public class ConfigurationExtensionsTest
    {
        [Theory]
        [InlineData("RabbitMq", MessageBrokers.RabbitMq)]
        public void When_Valid_Broker_Is_Configured_Should_Return_The_Expected(string brokerName, MessageBrokers expected)
        {
            var config = SetConfig(brokerName);

            var result = config.GetMessageBroker();
            
            result.Should().Be(expected);
        }        

        [Fact]
        public void When_Broker_Is_Not_Configured_Should_Return_Undefined_Value()
        {
            var config = new ConfigurationBuilder().Build();

            var broker = config.GetMessageBroker();

            var result = Enum.IsDefined(typeof(MessageBrokers), broker);
            result.Should().BeFalse();
        }
        
        [Fact]
        public void When_Invalid_Broker_Is_Configured_Should_Throws_The_Expected_Exception()
        {
            const string someInvalidBroker = "someInvalidBroker";
            var config = SetConfig(someInvalidBroker);

            Action ex = () => config.GetMessageBroker();
            ex.Should().Throw<InvalidOperationException>()
                       .WithMessage(string.Format(Resources.InvalidBrokerExceptionMessage, someInvalidBroker));
        }
        
        
        private static IConfigurationRoot SetConfig(string brokerName)
        {
            var dictionary = new Dictionary<string, string>
            {
                {EnvironmentConstants.MessageBroker, brokerName}
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(dictionary)
                .Build();
            return config;
        }
    }
}