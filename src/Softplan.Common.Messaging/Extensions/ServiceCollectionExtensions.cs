using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingManager(this IServiceCollection services, IConfiguration config, ILoggerFactory loggerFactory)
        {
            var builderFactory = new MessagingBuilderFactory();
            if (!builderFactory.HasConfiguration(config))
                return services;
            services.TryAddSingleton<IMessagingBuilderFactory, MessagingBuilderFactory>();
            services.TryAddSingleton<IBuilder>(provider => provider.GetService<IMessagingBuilderFactory>().GetBuilder(config, loggerFactory));
            services.TryAddScoped<IPublisher>(provider => provider.GetService<IBuilder>().BuildPublisher());
            services.TryAddSingleton<IMessagingManager>(provider => new MessagingManager(provider.GetService<IBuilder>(), loggerFactory));
            return services;
        }        
    }
}
