using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Elastic.Apm;
using Elastic.Apm.Config;
using ElasticApmPublisherSample.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Enuns;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.Extensions;

namespace ElasticApmPublisherSample
{
    class Program
    {
        private const string AppSettingsPublisher = "appsettings.json";
        private const string Text = "Message published with success.";
        private const string SimpleMessageDestination = "SimpleMessageDestination";
        private const string FibonacciDestination = "FibDestination";

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(Resources.StartingApplication);
                var config = GetConfiguration(AppSettingsPublisher);
                SetApmAgentConfiguration(config);
                string action;
                do
                {
                    action = GetAction();
                    ExecuteAction(action, config);
                } while (!string.Equals(action, "c", StringComparison.OrdinalIgnoreCase));
                Console.WriteLine(Resources.ApplicationClosed);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ApplicationError + " " + ex.Message);
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

        private static string GetAction()
        {
            string action;
            do
            {
                Console.WriteLine(Resources.OptionsMenu);
                action = Console.ReadLine();
            } while (!string.Equals(action, "1", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(action, "2", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(action, "c", StringComparison.OrdinalIgnoreCase));

            return action;
        }

        private static void ExecuteAction(string action, IConfiguration config)
        {
            if (action.Equals("1"))
                PublishMessage(config);
            if (action.Equals("2"))
                PublishMessageAndWaitResponse(config);
        }

        private static IPublisher GetPublisher(IConfiguration config)
        {
            var loggerFactory = new LoggerFactory();
            var messagingBuilderFactory = new MessagingBuilderFactory();
            var builder = messagingBuilderFactory.GetBuilder(config, loggerFactory);
            var publisher = builder.BuildPublisher();
            return publisher;
        }

        private static void PublishMessage(IConfiguration config)
        {
            var publisher = GetPublisher(config);
            string quit;
            do
            {
                Console.WriteLine(Resources.PublishingMessage);
                var method = new StackFrame().GetMethod();
                var message = GetSimpleMessage(method);
                publisher.Publish(message, SimpleMessageDestination);
                Console.WriteLine(Resources.SendMessageOrKill);
                quit = Console.ReadLine();
            } while (!string.Equals(quit, "b", StringComparison.OrdinalIgnoreCase));
        }

        private static SimpleMessage GetSimpleMessage(MemberInfo method)
        {
            var name = $"{method.DeclaringType}.{method.Name}";
            var message = new SimpleMessage
            {
                Text = Text,
                Headers = {[ApmConstants.TransactionName] = name} //Is recommended set the transaction name to be easy localize data.
            };
            return message;
        }

        private static void PublishMessageAndWaitResponse(IConfiguration config)
        {
            var publisher = GetPublisher(config);
            string quit;
            do
            {
                Console.WriteLine(Resources.WaitingFibonacciNumber);
                var value = quit = Console.ReadLine();
                if (string.Equals(value, "b", StringComparison.OrdinalIgnoreCase)) break;
                if (!GetValue(value, out var fibNum)) continue;
                Console.WriteLine(Resources.PublishingMessage);
                var method = new StackFrame().GetMethod();
                var message = GetFibMessage(method, fibNum);
                var reply = publisher.PublishAndWait<FibMessage>(message, FibonacciDestination).Result;
                Console.WriteLine(Resources.ResponseReceived);
                Console.WriteLine(string.IsNullOrEmpty(reply.ErrorMessage)
                    ? string.Format(Resources.FibonacciResult, fibNum, reply.Number)
                    : string.Format(Resources.FibonacciError, reply.ErrorMessage));
            } while (!string.Equals(quit, "b", StringComparison.OrdinalIgnoreCase));
        }

        private static bool GetValue(string value, out int fibNum)
        {
            if (int.TryParse(value, out fibNum)) return true;
            Console.WriteLine(Resources.InvalidIntValue, value);
            return false;
        }

        private static FibMessage GetFibMessage(MemberInfo method, int fibNum)
        {
            var name = $"{method.DeclaringType}.{method.Name}";
            var message = new FibMessage
            {
                Number = fibNum,
                Headers = {[ApmConstants.TransactionName] = name} //Is recommended set the transaction name to be easy localize data.
            };
            return message;
        }
    }
}