using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.AMQP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Softplan.Common.Messaging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingManager(this IServiceCollection services, IConfiguration config, ILoggerFactory loggerFactory, IConnectionFactory connectionFactory = null)
        {
            services.TryAddSingleton<IBuilder>(provider => new AmqpBuilder(config, loggerFactory, connectionFactory));
            services.TryAddScoped<IPublisher>(provider => provider.GetService<IBuilder>().BuildPublisher());
            services.TryAddSingleton<IMessagingManager>(provider => new MessagingManager(provider.GetService<IBuilder>(), loggerFactory));
            return services;
        }        
    }
}