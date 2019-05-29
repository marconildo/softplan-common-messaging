using FluentAssertions;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Xunit;

namespace Softplan.Common.Messaging.Abstractions.Tests
{
    public class EnvironmentConstantsTest
    {
        [Theory]
        [InlineData("MessageBroker", "MESSAGE_BROKER")]
        [InlineData("MessageBrokerUrl", "MESSAGE_BROKER_URL")]
        [InlineData("MessageBrokerApiUrl", "MESSAGE_BROKER_API_URL")]
        public void When_Get_a_Property_Value_Should_Return_The_Expected(string propertyName, string expected)
        {
            var value = typeof(EnvironmentConstants).GetProperty(propertyName).GetValue(null, null);

            value.Should().Be(expected);
        }
    }
}