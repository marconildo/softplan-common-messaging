using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Enuns;

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
        
        public static ApmProviders GetApmProvider(this IConfiguration configuration)
        {
            return configuration.GetValue<ApmProviders>(
                EnvironmentConstants.ApmProvider,
                default);
        }
    }
}