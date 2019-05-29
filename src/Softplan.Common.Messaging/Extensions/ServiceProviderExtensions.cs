using System;
using Microsoft.Extensions.DependencyInjection;
using Softplan.Common.Messaging.RabbitMq.Abstractions;

namespace Softplan.Common.Messaging.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider StartMessagingManager(this IServiceProvider provider)
        {
            var manager = provider.GetService<IMessagingManager>();
            manager.LoadProcessors(provider);
            manager.Start();
            return provider;
        }
    }
}