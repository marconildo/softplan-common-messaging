using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.RabbitMq.Abstractions;

namespace Softplan.Common.Messaging.Extensions
{
    public static class ConfigurationExtensions
    {
        public static MessageBrokers GetMessageBroker(this IConfiguration configuration)
        {
            return configuration.GetValue<MessageBrokers>(
                EnvironmentConstants.MessageBroker,
                default);
        }
    }
}