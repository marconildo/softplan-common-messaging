using System.Collections.Generic;
using Elastic.Apm.All;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.Configuration;
using Pose;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.ElasticApm.Constants;
using Softplan.Common.Messaging.ElasticApm.Extensions;
using Softplan.Common.Messaging.Tests.Helper;
using Xunit;

namespace Softplan.Common.Messaging.ElasticApm.Tests.Extensions
{
    public class ApplicationBuilderExtensionsTest
    {
        private readonly IApplicationBuilder _applicationBuilder;
        private readonly IConfigurationRoot _config;

        private Shim _useElasticApmShim;
        private Shim _elasticApmConstantsShim;
        
        private const string LogLevel = "LogLevel";
        private const string ServerUrls = "ServerUrls";
        private const string ServiceName = "ServiceName";
        private const string UseElasticApm = "UseElasticApm";
        private const string SetElasticApmConstants = "SetElasticApmConstants";
        
        private List<string> _calledExtensions;
        
        private Dictionary<string, string> _dictionary = new Dictionary<string, string>
        {
            {EnvironmentConstants.LogLevel, LogLevel},
            {EnvironmentConstants.ServerUrls, ServerUrls},
            {EnvironmentConstants.ServiceName, ServiceName}
        };
        
        public static IEnumerable<object[]> ConstantsToTest => new List<object[]>
        {
            new object[] { ElasticApmConstants.LogLevel, LogLevel },
            new object[] { ElasticApmConstants.ServerUrls, ServerUrls },
            new object[] { ElasticApmConstants.ServiceName, ServiceName }
        };

        public ApplicationBuilderExtensionsTest()
        {
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(_dictionary)
                .Build();
            _applicationBuilder = new ApplicationBuilder(null);
            _calledExtensions = new List<string>();
            ConfigureUseElasticApmShim(_calledExtensions);
            ConfigureElasticApmConstantsShim(_calledExtensions);
        }


        [Theory]
        [MemberData(nameof(ConstantsToTest))]
        public void When_SetElasticApmConstants_Should_Set_ElasticApmConstants(string constant, string value)
        {
            ApplicationBuilderExtensions.SetElasticApmConstants(_config);

            _config.GetValue<string>(constant).Should().Be(value);
        }
        
        [Fact]
        public void When_UseElasticApm_Should_Call_ApmMiddlewareExtension_UseElasticApm()
        {                        
            PoseSemaphoreHelper.Isolate(() =>
            {
                ApplicationBuilderExtensions.UseElasticApm(_applicationBuilder, _config);
            }, _useElasticApmShim, _elasticApmConstantsShim);

            _calledExtensions.Should().Contain(UseElasticApm);
        }
        
                
        private void ConfigureElasticApmConstantsShim(ICollection<string> extensionsCalleds)
        {
            _elasticApmConstantsShim = Shim.Replace(() =>ApplicationBuilderExtensions.SetElasticApmConstants(Is.A<IConfiguration>()))
                .With((IConfiguration configuration) =>
                {
                    extensionsCalleds.Add(SetElasticApmConstants);
                });
        }
        
        private void ConfigureUseElasticApmShim(ICollection<string> extensionsCalleds)
        {
            _useElasticApmShim = Shim.Replace(() => ApmMiddlewareExtension.UseElasticApm(Is.A<IApplicationBuilder>(), Is.A<IConfiguration>()))
                .With((IApplicationBuilder applicationBuilder, IConfiguration configuration) =>
                {
                    extensionsCalleds.Add(UseElasticApm);
                    return applicationBuilder;
                });
        }
    }
}