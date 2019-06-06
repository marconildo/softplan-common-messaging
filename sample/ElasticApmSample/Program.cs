using System;
using System.IO;
using System.Threading;
using Elastic.Apm;
using Elastic.Apm.Config;
using ElasticApmsample.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging;

namespace ElasticApmsample
{
    class Program
    {
        private const string AppSettingsPublisher = "appsettingsPublisher.json";
        private const string AppSettingsConsumer = "appsettingsConsumer.json";
        private const string Text = "Message published with success.";
        private const string Destination = "ElasticApmQueue";
        
        static void Main(string[] args)
        {
            try
            {                
                Console.WriteLine(Resources.StartingApplication);                                                
                PublishMessage();
                
                Thread.Sleep(10000);                
                ProccessQueueMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ApplicationError, ex.Message);
            }
        }        
                       
        
        private static void PublishMessage()
        {     
            var config = GetConfiguration(AppSettingsPublisher);
            SetApmAgentConfiguration(config);
            Agent.Tracer.CaptureTransaction("Main.RunTransaction", "Transaction", () =>
            {
                var random = new Random();
                Thread.Sleep(random.Next(500, 1000));
                RunSpan();
                RunSpan();
                
                Console.WriteLine(Resources.PublishingMessage); 
                var loggerFactory = new LoggerFactory();
                var messagingBuilderFactory = new MessagingBuilderFactory();
                var builder = messagingBuilderFactory.GetBuilder(config, loggerFactory);
                var publisher = builder.BuildPublisher();                                   
                publisher.Publish(new SampleMessage() { Text = Text }, Destination);
                
                RunSpan();
                RunSpan();
            });
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
            Console.WriteLine(Resources.SettingApmAgentConfiguration);
            var configurationReader = new ConfigurationReader(config) as IConfigurationReader;
            Agent.Setup(new AgentComponents(configurationReader: configurationReader));
        } 

        private static void RunSpan()
        {         
            var transacion = Agent.Tracer.CurrentTransaction;
            transacion.CaptureSpan("Main.RunSpan", "Span", () =>
            {
                var random = new Random();
                Thread.Sleep(random.Next(1500, 2000));
            });
        }
        
        private static void ProccessQueueMessage()
        {      
            var config = GetConfiguration(AppSettingsConsumer);
            //SetApmAgentConfiguration(config);
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
                Console.WriteLine(Resources.ApplicationClosed);
            }
        }
    }
}