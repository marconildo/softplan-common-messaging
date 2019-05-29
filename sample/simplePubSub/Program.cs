using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using simplePubSub.Properties;
using Softplan.Common.Messaging;
using Softplan.Common.Messaging.RabbitMq;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;

namespace simplePubSub
{
    class Program
    {
        private const string AppSettingsJson = "appsettings.json";
        private const string ItsWorking = "It's working";
        private const string Queue = "testQueue123";
        
        private static IConfiguration GetConfiguration()
        {            
            var builder = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile(AppSettingsJson, optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables();

            return builder.Build();
        }
        static void Main(string[] args)
        {
            Console.WriteLine(Resources.IniciandoAplicacao);
            try
            {
                ILoggerFactory factory = new LoggerFactory();
                var settings = GetConfiguration();

                IBuilder builder = new RabbitMqBuilder(settings, factory);
                using (var manager = new MessagingManager(builder, factory))
                {
                    var publisher = builder.BuildPublisher();

                    manager.LoadProcessors(null);
                    manager.Start();
                    Console.WriteLine(Resources.PublicandoMensagem);                    
                    publisher.Publish(new ExampleMessage() { Text = ItsWorking }, Queue);
                    Console.WriteLine(Resources.RodandoAplicacao);
                    Console.ReadLine();
                    manager.Stop();
                    Console.WriteLine(Resources.AplicacaoEncerrada);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(Resources.ErroAoExecutarAplicacao, err.Message);
            }
        }
    }
}
