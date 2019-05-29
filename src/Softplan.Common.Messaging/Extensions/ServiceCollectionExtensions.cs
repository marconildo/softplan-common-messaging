using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Softplan.Common.Messaging.RabbitMq;
using Softplan.Common.Messaging.RabbitMq.Abstractions;

namespace Softplan.Common.Messaging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingManager(this IServiceCollection services, IConfiguration config, ILoggerFactory loggerFactory, IConnectionFactory connectionFactory = null)
        {
            services.TryAddSingleton<IBuilder>(provider => new RabbitMqBuilder(config, loggerFactory, connectionFactory));
            services.TryAddScoped<IPublisher>(provider => provider.GetService<IBuilder>().BuildPublisher());
            services.TryAddSingleton<IMessagingManager>(provider => new MessagingManager(provider.GetService<IBuilder>(), loggerFactory));
            return services;
        }        
    }
}
