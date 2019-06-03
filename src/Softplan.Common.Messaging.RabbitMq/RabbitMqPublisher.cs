using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.RabbitMq.Properties;

namespace Softplan.Common.Messaging.RabbitMq
{
    public class RabbitMqPublisher : IPublisher
    {
        private readonly IModel _channel;
        private readonly ISerializer _serializer;
        private readonly IQueueApiManager _manager;
        
        private const string SendToDefaultQueue = "x-send-to-default-queue";
        private const string ContentEncoding = "utf-8";
        
        public RabbitMqPublisher(IModel channel, ISerializer serializer, IQueueApiManager manager)
        {
            _channel = channel;
            _serializer = serializer;
            _manager = manager;
        }        

        public void Publish(IMessage message, string destination = "", bool forceDestination = false)
        {
            destination = GetDestination(message, destination, forceDestination);
            _manager.EnsureQueue(destination);
            var messageProperties = GetBasicMessageProperties(message);
            var body = _serializer.Serialize(message, Encoding.UTF8);
            _channel.BasicPublish(string.Empty, destination, false, messageProperties, body);
        }        

        public Task<T> PublishAndWait<T>(IMessage message, string destination = "", bool forceDestination = false, int milliSecondsTimeout = 60000) where T : IMessage
        {
            return Task<T>.Factory.StartNew(() =>
            {
                var respQueue = new BlockingCollection<T>();
                var replyQueue = _channel.QueueDeclare(string.Empty).QueueName;
                message.ReplyTo = replyQueue;
                var consumerTag = _channel.BasicConsume(replyQueue, true, CreateConsumer(respQueue));
                return PublishAndWait(message, destination, forceDestination, milliSecondsTimeout, respQueue, consumerTag);
            });
        }       


        private static string GetDestination(IMessage message, string destination, bool forceDestination)
        {
            if (!forceDestination)
                destination = string.IsNullOrEmpty(message.ReplyQueue) ? destination : message.ReplyQueue;
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException(Resources.MessageDestionationIsNull);            
            return destination;
        }
        
        private IBasicProperties GetBasicMessageProperties(IMessage message)
        {
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            props.DeliveryMode = 2;            
            props.ContentEncoding = ContentEncoding;
            // required by Delphi lib currently
            props.Headers = props.Headers ?? new Dictionary<string, object>();            
            props.Headers[SendToDefaultQueue] = "false";
            if (!string.IsNullOrEmpty(message.ReplyTo))
                props.ReplyTo = message.ReplyTo;
            foreach (var header in message.Headers)
                props.Headers[header.Key] = header.Value;
            return props;
        }

        private IBasicConsumer CreateConsumer<T>(BlockingCollection<T> respQueue) where T : IMessage
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, eventArgs) =>
            {
                var message = _serializer.Deserialize<T>(Encoding.UTF8.GetString(eventArgs.Body));
                respQueue.Add(message);
            };
            return consumer;
        }
        
        private T PublishAndWait<T>(IMessage message, string destination, bool forceDestination, int milliSecondsTimeout, BlockingCollection<T> respQueue, string consumerTag) where T : IMessage
        {
            try
            {
                Publish(message, destination, forceDestination);
                if (respQueue.TryTake(out var reply, milliSecondsTimeout))
                    return reply;
                throw new TimeoutException(Resources.ReplyMessageNotReceived);
            }
            finally
            {
                _channel.BasicCancel(consumerTag);
            }
        }
    }
}
