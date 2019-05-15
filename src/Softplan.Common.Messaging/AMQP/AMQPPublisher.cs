using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Softplan.Common.Messaging.AMQP
{
    public class AmqpPublisher : IPublisher
    {
        private readonly IModel channel;
        private readonly ISerializer serializer;
        private readonly IQueueApiManager manager;
        public AmqpPublisher(IModel channel, ISerializer serializer, IQueueApiManager manager)
        {
            this.channel = channel;
            this.serializer = serializer;
            this.manager = manager;
        }

        private IBasicProperties GetBasicMessageProperties(IMessage message)
        {
            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.DeliveryMode = 2;
            props.ContentEncoding = "utf-8";
            // required by Delphi lib currently
            props.Headers = props.Headers ?? new Dictionary<string, object>();
            props.Headers["x-send-to-default-queue"] = "false";

            if (!String.IsNullOrEmpty(message.ReplyTo))
                props.ReplyTo = message.ReplyTo;

            foreach (var h in message.Headers)
                props.Headers[h.Key] = h.Value;

            return props;
        }

        public void Publish(IMessage message, string destination = "", bool forceDestination = false)
        {
            string _destination;
            if (forceDestination)
                _destination = destination;
            else
                _destination = String.IsNullOrEmpty(message.ReplyQueue) ? destination : message.ReplyQueue;

            if (String.IsNullOrEmpty(_destination))
                throw new ArgumentException("Destination cannot be empty.");

            manager.EnsureQueue(_destination);
            channel.BasicPublish("", _destination, false, GetBasicMessageProperties(message), serializer.Serialize(message, Encoding.UTF8));
        }

        public Task<T> PublishAndWait<T>(IMessage message, string destination = "", bool forceDestination = false, int millisecodsTimeout = 60000) where T : IMessage
        {
            return Task<T>.Factory.StartNew(() =>
            {
                var respQueue = new BlockingCollection<T>();

                var replyQueue = channel.QueueDeclare(String.Empty, false, true, true).QueueName;
                message.ReplyTo = replyQueue;
                var consumerTag = channel.BasicConsume(replyQueue, true, CreateConsumer(respQueue));
                try
                {
                    Publish(message, destination, forceDestination);

                    if (!respQueue.TryTake(out T reply, millisecodsTimeout))
                        throw new TimeoutException("A resposta da mensagem não foi recebida");

                    return reply;
                }
                finally
                {
                    channel.BasicCancel(consumerTag);
                }
            });
        }

        private IBasicConsumer CreateConsumer<T>(BlockingCollection<T> respQueue) where T : IMessage
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                T message = serializer.Deserialize<T>(Encoding.UTF8.GetString(ea.Body));
                respQueue.Add(message);
            };

            return consumer;
        }
    }
}
