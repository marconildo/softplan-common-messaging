using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Softplan.Common.Messaging.Abstractions.Enuns;
using Softplan.Common.Messaging.ElasticApm.Extensions;

namespace Softplan.Common.Messaging.Extensions
{
    internal static class ApmExtensions
    {
        public static IServiceCollection AddApm(this IServiceCollection serviceCollection, 
            IConfiguration configuration)
        {
            var provider = configuration.GetApmProvider();
            return Enum.IsDefined(typeof(ApmProviders), provider)
                ? Lookup[provider].Add(serviceCollection, configuration)
                : serviceCollection;
        }

        public static void UseApm(this IApplicationBuilder builder, IConfiguration configuration)
        {
            var provider = configuration.GetApmProvider();
            if (Enum.IsDefined(typeof(ApmProviders), provider))
                Lookup[provider].Use(builder, configuration);
        }

        private static readonly IDictionary<ApmProviders, ApmProviderActions> Lookup =
            new Dictionary<ApmProviders, ApmProviderActions>
            {
                [ApmProviders.ElasticApm] = new ApmProviderActions
                {
                    Add = (services, _) => services,
                    Use = (builder, configuration) => builder.UseElasticApm(configuration)
                }
            };
    }
}