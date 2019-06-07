using System;
using System.IO;
using Elastic.Apm;
using Elastic.Apm.Config;
using ElasticApmPublisherSample.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging;
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
                var action = GetAction();
                ExecuteAction(action, config);
                Console.WriteLine(Resources.ApplicationClosed);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ApplicationError, ex.Message);
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
        /// Faz a configuração do agente para utilização em ConcoleApplications.
        ///
        /// Obs.:
        /// A configuração do agente deve ser feita na aplicação, de acordo com a documentação disponível em https://www.elastic.co/guide/en/apm/agent/dotnet/current/index.html.
        /// O componente de mensageria suporta o controle distribuido de transações, porém não é responsável por fazer a configuração do agente.
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
                Console.WriteLine(Resources.ActionType);
                action = Console.ReadLine();
            } while (!string.Equals(action, "1", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(action, "2", StringComparison.OrdinalIgnoreCase));

            return action;
        }
        
        private static void ExecuteAction(string action, IConfiguration config)
        {
            if (action.Equals("1"))
                PublishMessage(config);
            if (action.Equals("2"))
                PublishMessageAndWaitResponse(config);
        }

        private static void PublishMessage(IConfiguration config)
        {                                          
            var publisher = GetPublisher(config);
            string quit;
            do
            {
                Console.WriteLine(Resources.PublishingMessage);
                publisher.Publish(new SimpleMessage {Text = Text}, SimpleMessageDestination);
                Console.WriteLine(Resources.ClosingApplication);
                quit = Console.ReadLine();
            } while (!string.Equals(quit, "c", StringComparison.OrdinalIgnoreCase));
        }        

        private static void PublishMessageAndWaitResponse(IConfiguration config)
        {                                         
            var publisher = GetPublisher(config);
            var quit = string.Empty;
            do
            {
                Console.WriteLine(Resources.WaitingFibonacciNumber);
                var value = Console.ReadLine();
                if (string.Equals(value, "c", StringComparison.OrdinalIgnoreCase)) break;
                if (!GetValue(value, out var fibNum)) continue;  
                Console.WriteLine(Resources.PublishingMessage); 
                var reply = publisher.PublishAndWait<FibMessage>(new FibMessage {Number = fibNum}, FibonacciDestination).Result;
                Console.WriteLine(Resources.ResponseReceived); 
                Console.WriteLine(string.IsNullOrEmpty(reply.ErrorMessage)
                    ? string.Format(Resources.FibonacciResult, fibNum, reply.Number)
                    : string.Format(Resources.FibonacciError, reply.ErrorMessage));                                               
                Console.WriteLine(Resources.ClosingApplication);
                quit = Console.ReadLine();
            } while (!string.Equals(quit, "c", StringComparison.OrdinalIgnoreCase));
        }        

        private static IPublisher GetPublisher(IConfiguration config)
        {
            var loggerFactory = new LoggerFactory();
            var messagingBuilderFactory = new MessagingBuilderFactory();
            var builder = messagingBuilderFactory.GetBuilder(config, loggerFactory);
            var publisher = builder.BuildPublisher();
            return publisher;
        }
        
        private static bool GetValue(string value, out int fibNum)
        {            
            if (int.TryParse(value, out fibNum)) return true;
            Console.WriteLine(Resources.InvalidIntValue, value);
            return false;
        }
    }
}