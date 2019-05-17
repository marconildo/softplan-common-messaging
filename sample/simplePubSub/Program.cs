using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.AMQP;
using System.IO;
using Softplan.Common.Messaging;

namespace simplePubSub
{
    class Program
    {
        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables();

            return builder.Build();
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando aplicação");
            try
            {
                ILoggerFactory factory = new LoggerFactory();
                var settings = GetConfiguration();

                IBuilder builder = new AmqpBuilder(settings, factory);
                using (var manager = new MessagingManager(builder, factory))
                {
                    var publisher = builder.BuildPublisher();

                    manager.LoadProcessors(null);
                    manager.Start();
                    Console.WriteLine("[.] Publicando mensagem para [testQueue123] ...");
                    publisher.Publish(new ExampleMessage() { Text = "It's working" }, "testQueue123");
                    Console.WriteLine("Rodando aplicação. Pressione (enter) para encerrar.");
                    Console.ReadLine();
                    manager.Stop();
                    Console.WriteLine("Aplicação encerrada com sucesso.");
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"Erro ao executar aplicação. Detalhes: ${err.Message}");
            }
        }
    }
}
