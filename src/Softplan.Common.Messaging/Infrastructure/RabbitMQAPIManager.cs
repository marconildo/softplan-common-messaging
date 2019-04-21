using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.AMQP;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Softplan.Common.Messaging.Infrastructure
{
    public class RabbitMQApiManager : IQueueApiManager
    {
        private readonly string url;
        private readonly IModel channel;
        private readonly HttpClient client;

        private string GetQueueResourceUrl(string queueName, string vHost = "%2f")
        {
            return $"queues/{vHost}/{queueName}?columns=durable,auto_delete,exclusive,arguments";
        }
        private string GetUrl(string resourceUrl)
        {
            return this.url + resourceUrl;
        }
        public RabbitMQApiManager(string url, string user, string password, IModel channel, HttpClient client = null)
        {
            this.url = url.EndsWith("/") ? url : url + "/";
            this.channel = channel;
            this.client = client ?? new HttpClient();
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{user}:{password}"))
            );
        }

        public QueueInfo GetQueueInfo(string queueName)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, this.GetUrl(GetQueueResourceUrl(queueName)));
            var response = client.SendAsync(request).Result;

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new QueueInfo
                    {
                        Durable = true,
                        AutoDelete = false,
                        Exclusive = false,
                        Priority = 0
                    };

                case HttpStatusCode.OK:
                    return JsonConvert.DeserializeObject<QueueInfo>(response.Content.ReadAsStringAsync().Result);

                default:
                    throw new HttpRequestException($"Erro ao consultar API do RabbitMQ. Status {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}");
            }
        }

        public string EnsureQueue(string queueName)
        {

            if (new Regex(@"^amq.").IsMatch(queueName))
                return queueName;

            var info = this.GetQueueInfo(queueName);
            var resp = channel.QueueDeclare(
                queue: queueName,
                durable: info.Durable,
                exclusive: info.Exclusive,
                autoDelete: info.AutoDelete,
                arguments: info.Arguments
            );

            return resp.QueueName;
        }
    }
}
