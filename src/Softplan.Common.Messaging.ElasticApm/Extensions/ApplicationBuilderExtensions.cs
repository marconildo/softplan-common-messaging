using System;
using Elastic.Apm.All;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.ElasticApm.Constants;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Constants;

namespace Softplan.Common.Messaging.ElasticApm.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseElasticApm(this IApplicationBuilder builder, IConfiguration configuration)
        {
            try
            {
                SetElasticApmConstants(configuration);
                return ApmMiddlewareExtension.UseElasticApm(builder, configuration);
            }
            catch (Exception)
            {
                return builder;
            }
        }

        public static void SetElasticApmConstants(IConfiguration configuration)
        {
            configuration[ElasticApmConstants.LogLevel] =
                configuration.GetValue(EnvironmentConstants.LogLevel, string.Empty);
            configuration[ElasticApmConstants.ServerUrls] =
                configuration.GetValue(EnvironmentConstants.ServerUrls, string.Empty);
            configuration[ElasticApmConstants.ServiceName] =
                configuration.GetValue(EnvironmentConstants.ServiceName, string.Empty);
        }
    }
}