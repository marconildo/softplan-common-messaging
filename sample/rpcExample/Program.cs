using System;
using Softplan.Common.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using rpcExample.Properties;
using Softplan.Common.Messaging.RabbitMq;
using Softplan.Common.Messaging.RabbitMq.Abstractions;

namespace rpcExample
{
    class Program
    {
        private const string AppSettingsJson = "appsettings.json";
        private const string TestFibonacci = "test.fibonacci";
        private const string Quit = "quit";
        
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
                IBuilder builder = new RabbitMqBuilder(GetConfiguration(), factory);
                var publisher = builder.BuildPublisher();                
                var manager = new MessagingManager(builder, factory);               
                manager.LoadProcessors(null);
                manager.Start();               
                PubliqueMensagem(publisher);
                manager.Stop();
                Console.WriteLine(Resources.AplicacaoEncerrada);
            }
            catch (Exception err)
            {
                Console.WriteLine(Resources.ErroAoExecutarAplicacao, err.Message);
            }
        }

        private static void PubliqueMensagem(IPublisher publisher)
        {
            Console.WriteLine(Resources.PublicandoMensagem);
            Console.WriteLine(Resources.RodandoAplicacao);
            while (true)
            {
                var value = Console.ReadLine();
                if (value == Quit) break;
                if (!int.TryParse(value, out var fibNum))
                {
                    Console.WriteLine(Resources.InteiroInvalido, value);
                    continue;
                }

                var reply = publisher.PublishAndWait<FibMessage>(new FibMessage() {Number = fibNum}, TestFibonacci).Result;
                Console.WriteLine(string.IsNullOrEmpty(reply.ErrorMessage)
                    ? string.Format(Resources.ResultadoFibonacci, fibNum, reply.Number)
                    : string.Format(Resources.ErroCalcularFibonacci, reply.ErrorMessage));
            }
        }
    }
}
