using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Enuns;
using Softplan.Common.Messaging.Extensions;
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
            var config = SetConfigToBrokerTest(brokerName);

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
            var config = SetConfigToBrokerTest(someInvalidBroker);

            Action ex = () => config.GetMessageBroker();
            ex.Should().Throw<InvalidOperationException>()
                       .WithMessage(string.Format(Resources.InvalidBrokerExceptionMessage, someInvalidBroker));
        }
        
        
        [Theory]
        [InlineData("ElasticApm", ApmProviders.ElasticApm)]
        public void When_Valid_Provider_Is_Configured_Should_Return_The_Expected(string providerName, ApmProviders expected)
        {
            var config = SetConfigToApmProviderTest(providerName);

            var result = config.GetApmProvider();

            result.Should().Be(expected);
        }
        
        [Fact]
        public void When_Provider_Is_Not_Configured_Should_Return_Undefined_Value()
        {
            var config = new ConfigurationBuilder().Build();

            var provider = config.GetApmProvider();

            var result = Enum.IsDefined(typeof(ApmProviders), provider);
            result.Should().BeFalse();
        }
        
        [Fact]
        public void When_Invalid_Provider_Is_Configured_Should_Throws_The_Expected_Exception()
        {
            const string someInvalidProvider = "someInvalidProvider";
            var config = SetConfigToApmProviderTest(someInvalidProvider);

            Action ex = () => config.GetApmProvider();
            ex.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(Resources.InvalidProviderExceptionMessage);
        }
        
                
        private static IConfigurationRoot SetConfigToBrokerTest(string brokerName)
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
        
        private static IConfigurationRoot SetConfigToApmProviderTest(string providerName)
        {
            var dictionary = new Dictionary<string, string>
            {
                {EnvironmentConstants.ApmProvider, providerName}
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(dictionary)
                .Build();
            return config;
        }
    }
}