using System;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.AMQP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Softplan.Common.Messaging.Extensions
{
    public static class BootstrapperExtensions
    {
        public static IServiceCollection AddMessagingManager(this IServiceCollection services, IConfiguration config, ILoggerFactory loggerFactory)
        {
            services.TryAddSingleton<IBuilder>(provider => new AmqpBuilder(config, loggerFactory));
            services.TryAddScoped<IPublisher>(provider => provider.GetService<IBuilder>().BuildPublisher());
            services.TryAddSingleton<IMessagingManager>(provider => new MessagingManager(provider.GetService<IBuilder>(), loggerFactory));
            return services;
        }

        public static IServiceProvider StartMessagingManager(this IServiceProvider provider)
        {
            var manager = provider.GetService<IMessagingManager>();
            manager.LoadProcessors(provider);
            manager.Start();
            return provider;
        }
    }
}
