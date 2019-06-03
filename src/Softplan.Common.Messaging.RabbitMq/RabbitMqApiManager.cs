using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.RabbitMq.Properties;

namespace Softplan.Common.Messaging.RabbitMq
{
    public class RabbitMqApiManager : IQueueApiManager
    {
        private const string Scheme = "Basic";
        private const string QueryString = "columns=durable,auto_delete,exclusive,arguments";
        private readonly string _url;
        private readonly IModel _channel;
        private readonly HttpClient _client;        
        
        public RabbitMqApiManager(string url, string user, string password, IModel channel, HttpClient client = null)
        {
            _url = url.EndsWith("/") ? url : url + "/";
            _channel = channel;
            _client = client ?? new HttpClient();            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                Scheme, Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{user}:{password}"))
            );
        }

        public QueueInfo GetQueueInfo(string queueName)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, GetUrl(GetQueueResourceUrl(queueName)));
            var response = _client.SendAsync(request).Result;
            return ProccessResponse(response);
        }        

        public string EnsureQueue(string queueName)
        {
            if (new Regex(@"^amq.").IsMatch(queueName))
                return queueName;

            var info = GetQueueInfo(queueName);
            var resp = _channel.QueueDeclare(
                queueName,
                info.Durable,
                info.Exclusive,
                info.AutoDelete,
                info.Arguments
            );

            return resp.QueueName;
        }
        
        private string GetUrl(string resourceUrl)
        {
            return _url + resourceUrl;
        }
        
        private static string GetQueueResourceUrl(string queueName, string vHost = "%2f")
        {
            return $"queues/{vHost}/{queueName}?{QueryString}";
        }               
        
        private static QueueInfo ProccessResponse(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {                
                case HttpStatusCode.OK:
                    return JsonConvert.DeserializeObject<QueueInfo>(response.Content.ReadAsStringAsync().Result);                
                case HttpStatusCode.NotFound:
                    return GetQueueInfo();                
                default:
                    throw new HttpRequestException(string.Format(Resources.RabbitMQAPIError, response.StatusCode, response.Content.ReadAsStringAsync().Result));
            }
        }

        private static QueueInfo GetQueueInfo()
        {
            return new QueueInfo
            {
                Durable = true,
                AutoDelete = false,
                Exclusive = false,
                Priority = 0
            };
        }
    }
}
