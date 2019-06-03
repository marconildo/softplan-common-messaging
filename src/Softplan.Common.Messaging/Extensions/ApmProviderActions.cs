using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Softplan.Common.Messaging.Extensions
{
    internal class ApmProviderActions
    {
        public Func<IServiceCollection, IConfiguration, IServiceCollection> Add { get; set; }
        public Action<IApplicationBuilder, IConfiguration> Use { get; set; } 
    }
}