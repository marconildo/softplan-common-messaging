using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Constants;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.RabbitMq.Properties;

namespace Softplan.Common.Messaging.RabbitMq
{
    public class RabbitMqBuilder : IBuilder, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly IMessagingWorkersFactory _messagingWorkersFactory;
        private readonly ILogger _logger;
        private IConnection _connection;
        private IQueueApiManager _apiManager;

        public IDictionary<string, Type> MessageQueueMap { get; }
        public IConnectionFactory ConnectionFactory { get; set; }

        private IConnection Connection
        {
            get
            {
                if (_connection != null)
                    return _connection;
                ConnectionFactory = ConnectionFactory ?? GetConnectionFactory1();
                _connection = ConnectionFactory.CreateConnection();
                return _connection;
            }
        }

        private const string Guest = "guest";

        public RabbitMqBuilder(IConfiguration config, ILoggerFactory loggerFactory, IMessagingWorkersFactory messagingWorkersFactory)
        {
            _config = config;
            _messagingWorkersFactory = messagingWorkersFactory;
            _logger = loggerFactory.CreateLogger<RabbitMqBuilder>();
            MessageQueueMap = new Dictionary<string, Type>();
        }

        public IQueueApiManager BuildApiManager()
        {
            if (_apiManager != null)
                return _apiManager;

            _logger.LogTrace(Resources.APIManagerCreating);
            var url = _config.GetValue<string>(EnvironmentConstants.MessageBrokerApiUrl);
            var (user, password) = GetUserData(url);
            var channel = Connection.CreateModel();
            _apiManager = new RabbitMqApiManager(url, user, password, channel);

            return _apiManager;
        }

        public IConsumer BuildConsumer()
        {
            var channel = Connection.CreateModel();
            return new RabbitMqConsumer(channel, InternalBuildPublisher(channel), this, BuildApiManager(), _messagingWorkersFactory.GetMessageProcessor(_config));
        }

        public IMessage BuildMessage(string queue, int version, string data = null)
        {
            if (!MessageQueueMap.ContainsKey(queue))
            {
                throw new KeyNotFoundException(string.Format(Resources.NoMessagesMappedToQueue, queue));
            }
            return BuildSerializer().Deserialize(MessageQueueMap[queue], data);
        }

        public IPublisher BuildPublisher()
        {
            return InternalBuildPublisher(Connection.CreateModel());
        }

        public ISerializer BuildSerializer()
        {
            return new MessageSerializer();
        }

        private IPublisher InternalBuildPublisher(IModel channel)
        {
            return new RabbitMqPublisher(channel, BuildSerializer(), BuildApiManager(), _messagingWorkersFactory.GetMessagePublisher(_config));
        }

        private static (string, string) GetUserData(string url)
        {
            var parser = new Uri(url);
            var userInfo = parser.UserInfo.Split(':');
            var user = userInfo.Length >= 1 && !string.IsNullOrEmpty(userInfo[0]) ? userInfo[0] : Guest;
            var password = userInfo.Length >= 2 ? userInfo[1] : Guest;
            return (user, password);
        }

        private ConnectionFactory GetConnectionFactory1()
        {
            return new ConnectionFactory { Uri = new Uri(_config.GetValue<string>(EnvironmentConstants.MessageBrokerUrl)) };
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
