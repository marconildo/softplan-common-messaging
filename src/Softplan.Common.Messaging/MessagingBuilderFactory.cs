using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Extensions;
using Softplan.Common.Messaging.Properties;
using Softplan.Common.Messaging.RabbitMq;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Constants;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;

namespace Softplan.Common.Messaging
{
    public class MessagingBuilderFactory : IMessagingBuilderFactory
    {                
        private MessageBrokers _messageBroker;
        private string _urlBroker;
        private string _urlApiBroker;

        public bool HasConfiguration(IConfiguration config)
        {
            LoadConfigurations(config);
            return(Enum.IsDefined(typeof(MessageBrokers), _messageBroker) && !string.IsNullOrEmpty(_urlBroker) && !string.IsNullOrEmpty(_urlApiBroker));
        }
        
        public IBuilder GetBuilder(IConfiguration config, ILoggerFactory loggerFactory)
        {
            if (!HasConfiguration(config))
                throw new ConfigurationErrorsException(Resources.AmqpConfigurationNotFound);
            switch (_messageBroker)
            {
                case MessageBrokers.RabbitMq:
                    return new RabbitMqBuilder(config, loggerFactory);
                default:
                    throw new ArgumentOutOfRangeException(Resources.InvalidAmqpBroker);
            }
        }
                
        
        private void LoadConfigurations(IConfiguration config)
        {
            _messageBroker = config.GetMessageBroker();
            _urlBroker = config.GetValue<string>(EnvironmentConstants.MessageBrokerUrl);
            _urlApiBroker = config.GetValue<string>(EnvironmentConstants.MessageBrokerApiUrl);
        }
    }
}