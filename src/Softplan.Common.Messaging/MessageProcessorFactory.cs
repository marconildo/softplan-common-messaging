using System;
using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Enuns;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.ElasticApm;
using Softplan.Common.Messaging.Extensions;
using Softplan.Common.Messaging.Properties;

namespace Softplan.Common.Messaging
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        private ApmProviders _apmProvider;

        
        public IMessageProcessor GetMessageProcessor(IConfiguration config)
        {
            if (!HasConfiguration(config))
                return new DefaultMessageProcessor();
            switch (_apmProvider)
            {
                case ApmProviders.ElasticApm:
                    return new ElasticApmMessageProcessor();
                default:
                    throw new ArgumentOutOfRangeException(Resources.InvalidApmProvider);
            }
        }
        
        
        private bool HasConfiguration(IConfiguration config)
        {
            _apmProvider = config.GetApmProvider();
            return(Enum.IsDefined(typeof(ApmProviders), _apmProvider));
        }
    }
}