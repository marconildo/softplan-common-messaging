using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Infrastructure;
using Softplan.Common.Messaging.Properties;

namespace Softplan.Common.Messaging.AMQP
{
    public class AmqpBuilder : IBuilder, IDisposable
    {
        private readonly IConfiguration _appSettings;
        private readonly ILogger _logger;
        private readonly IConnection _connection;
        private IQueueApiManager _apiManager;

        public IDictionary<string, Type> MessageQueueMap { get; }
        
        private const string RabbitUrlKey = "RABBIT_URL";
        private const string RabbitApiUrlKey = "RABBIT_API_URL";

        public AmqpBuilder(IConfiguration appSettings, ILoggerFactory loggerFactory, IConnectionFactory connectionFactory = null)
        {
            _appSettings = appSettings;
            _logger = loggerFactory.CreateLogger<AmqpBuilder>();

            MessageQueueMap = new Dictionary<string, Type>();
            var factory = connectionFactory ?? new ConnectionFactory { Uri = new Uri(appSettings.GetValue<string>(RabbitUrlKey)) };

            _connection = factory.CreateConnection();
        }

        public IQueueApiManager BuildApiManager()
        {
            if (_apiManager != null)
                return _apiManager;

            _logger.LogTrace(Resources.APIManagerCreating);
            var parser = new Uri(_appSettings.GetValue<string>(RabbitApiUrlKey));
            var userInfo = parser.UserInfo.Split(':');
            var user = userInfo.Length >= 1 && !string.IsNullOrEmpty(userInfo[0]) ? userInfo[0] : "guest";
            var password = userInfo.Length >= 2 ? userInfo[1] : "guest";
            _apiManager = new RabbitMqApiManager(_appSettings.GetValue<string>(RabbitApiUrlKey), user, password, _connection.CreateModel());

            return _apiManager;
        }

        public IConsumer BuildConsumer()
        {
            var channel = _connection.CreateModel();
            return new AmqpConsumer(channel, InternalBuildPublisher(channel), this, BuildApiManager());
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
            return InternalBuildPublisher(_connection.CreateModel());
        }

        public ISerializer BuildSerializer()
        {
            return new MessageSerializer();
        }

        private IPublisher InternalBuildPublisher(IModel channel)
        {
            return new AmqpPublisher(channel, BuildSerializer(), BuildApiManager());
        }

        #region IDisposable Support
        private bool disposedValue; // Para detectar chamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _connection.Close();
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
