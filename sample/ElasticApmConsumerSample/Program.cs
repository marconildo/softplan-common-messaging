﻿using System;
using System.IO;
using Elastic.Apm;
using Elastic.Apm.Config;
using ElasticApmConsumerSample.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging;
using Softplan.Common.Messaging.Abstractions.Enuns;
using Softplan.Common.Messaging.Extensions;

namespace ElasticApmConsumerSample
{
    class Program
    {
        private const string AppSettingsConsumer = "appsettings.json";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(Resources.StartingApplication);
                var config = GetConfiguration(AppSettingsConsumer);
                SetApmAgentConfiguration(config);
                ProccessQueueMessage(config);
                Console.WriteLine(Resources.ApplicationClosed);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ApplicationError + " " + ex.Message);
            }
        }

        private static void ProccessQueueMessage(IConfiguration config)
        {
            var loggerFactory = new LoggerFactory();
            var messagingBuilderFactory = new MessagingBuilderFactory();
            var builder = messagingBuilderFactory.GetBuilder(config, loggerFactory);
            using (var manager = new MessagingManager(builder, loggerFactory))
            {
                manager.LoadProcessors(null);
                manager.Start();
                Console.WriteLine(Resources.ClosingApplication);
                Console.ReadLine();
                manager.Stop();
            }
        }

        private static IConfiguration GetConfiguration(string settings)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(settings, true, true)
                .AddEnvironmentVariables();
            return builder.Build();
        }

        /// <summary>
        /// Obs.:
        /// The configuration of the agent must be done in the application, according to the available documentation: https://www.elastic.co/guide/en/apm/agent/dotnet/current/index.html.
        /// The messaging component supports distributed transaction control, but is not responsible for doing the agent configuration.
        /// </summary>
        private static void SetApmAgentConfiguration(IConfiguration config)
        {
            var apmProvider = config.GetApmProvider();
            if (!Enum.IsDefined(typeof(ApmProviders), apmProvider) || apmProvider != ApmProviders.ElasticApm) return;
            Console.WriteLine(Resources.SettingApmAgentConfiguration);
            var configurationReader = new ConfigurationReader(config) as IConfigurationReader;
            Agent.Setup(new AgentComponents(configurationReader: configurationReader));
        }
    }
}