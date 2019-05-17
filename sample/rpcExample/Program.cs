using System;
using System.Collections.Specialized;
using Softplan.Common.Messaging;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.AMQP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace rpcExample
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

                IBuilder builder = new AmqpBuilder(GetConfiguration(), factory);
                var manager = new MessagingManager(builder, factory);
                var publisher = builder.BuildPublisher();

                manager.LoadProcessors(null);
                manager.Start();
                Console.WriteLine("[.] Publicando mensagem para [testQueue123] ...");
                Console.WriteLine("Rodando aplicação. Digite (quit) para encerrar ou qualquer numero apra calcular Fibonacci.");
                while (true)
                {
                    var value = Console.ReadLine();
                    if (value == "quit") break;
                    if (!Int32.TryParse(value, out int fibNum))
                        Console.WriteLine($"{value} não é um inteiro válido");

                    var reply = publisher.PublishAndWait<FibMessage>(new FibMessage() { Number = fibNum }, "test.fibonacci").Result;
                    if (String.IsNullOrEmpty(reply.ErrorMessage))
                        Console.WriteLine($"Fibonacci para {fibNum} é igual a {reply.Number}");
                    else
                        Console.WriteLine($"Erro ao calcular fibonacci: {reply.ErrorMessage}");
                }

                manager.Stop();
                Console.WriteLine("Aplicação encerrada com sucesso.");
            }
            catch (Exception err)
            {
                Console.WriteLine($"Erro ao executar aplicação. Detalhes: ${err.Message}");
            }
        }
    }
}
