using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Softplan.Common.Messaging.AMQP
{
    public class AmqpBuilder : IBuilder, IDisposable
    {
        private readonly IConfiguration appSettings;
        private readonly ILogger logger;
        private readonly IConnection connection;
        private IQueueApiManager apiManager = null;

        public IDictionary<string, Type> MessageQueueMap { get; private set; }

        public AmqpBuilder(IConfiguration appSettings, ILoggerFactory loggerFactory, IConnectionFactory connectionFactory = null)
        {
            this.appSettings = appSettings;
            logger = loggerFactory.CreateLogger<AmqpBuilder>();

            MessageQueueMap = new Dictionary<string, Type>();
            var factory = connectionFactory ?? new ConnectionFactory() { Uri = new Uri(appSettings.GetValue<string>("RABBIT_URL")) };

            connection = factory.CreateConnection();
        }

        public IQueueApiManager BuildAPIManager()
        {
            if (apiManager != null)
            {
                return apiManager;
            }

            logger.LogTrace("Creating a new API Manager instance.");
            var parser = new Uri(appSettings.GetValue<string>("RABBIT_API_URL"));
            var userInfo = parser.UserInfo.Split(new[] { ':' });
            var user = userInfo.Length >= 1 && !string.IsNullOrEmpty(userInfo[0]) ? userInfo[0] : "guest";
            var password = userInfo.Length >= 2 ? userInfo[1] : "guest";
            apiManager = new RabbitMQApiManager(appSettings.GetValue<string>("RABBIT_API_URL"),
                user,
                password,
                connection.CreateModel());

            return apiManager;
        }

        public IConsumer BuildConsumer()
        {
            var channel = connection.CreateModel();
            return new AmqpConsumer(channel, InternalBuildPublisher(channel), this, BuildAPIManager());
        }

        public IMessage BuildMessage(string queue, int version, string data = null)
        {
            if (!MessageQueueMap.ContainsKey(queue))
            {
                throw new KeyNotFoundException($"Não existe nenhuma mensagem mapeada para a fila {queue}");
            }
            return BuildSerializer().Deserialize(MessageQueueMap[queue], data);
        }

        public IPublisher BuildPublisher()
        {
            return InternalBuildPublisher(connection.CreateModel());
        }

        public ISerializer BuildSerializer()
        {
            return new JsonSerializer();
        }

        private IPublisher InternalBuildPublisher(IModel channel)
        {
            return new AmqpPublisher(channel, BuildSerializer(), BuildAPIManager());
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar chamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connection.Close();
                }

                disposedValue = true;
            }
        }

        // Código adicionado para implementar corretamente o padrão IDiposable.
        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza em Dispose(bool disposing) acima.
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
