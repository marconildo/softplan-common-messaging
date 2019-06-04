using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Softplan.Common.Messaging.Abstractions.Interfaces;
using Softplan.Common.Messaging.RabbitMq.Properties;

namespace Softplan.Common.Messaging.RabbitMq
{
    public class RabbitMqConsumer : IConsumer
    {
        private readonly IModel _channel;
        private readonly IPublisher _publisher;
        private readonly IBuilder _builder;
        private readonly IQueueApiManager _manager;
        private readonly IMessageProcessor _messageProcessor;
        public string ConsumerTag { get; private set; }
        
        private const string ParamName = "queue";

        public RabbitMqConsumer(IModel channel, IPublisher publisher, IBuilder builder, IQueueApiManager manager, IMessageProcessor messageProcessor)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _messageProcessor = messageProcessor;
            ConsumerTag = string.Empty;
        }

        public void Start(IProcessor processor, string queue)
        {
            ValidateConsumerStarted();
            ValidateQueueName(queue);
            _manager.EnsureQueue(queue);
            _channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, args) => OnMessageReceived(processor, queue, args);
            ConsumerTag = _channel.BasicConsume(queue, false, consumer);
        }
        
        public void Stop()
        {
            if (!string.IsNullOrEmpty(ConsumerTag))
                _channel.BasicCancel(ConsumerTag);
            ConsumerTag = string.Empty;
        }

        protected void OnMessageReceived(IProcessor processor, string queue, BasicDeliverEventArgs args)
        {
            var message = _builder.BuildMessage(queue, 1, Encoding.UTF8.GetString(args.Body));
            try
            {
                SetReplyQueue(args, message);
                _messageProcessor.ProcessMessage(message, _publisher, processor.ProcessMessage);
                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                ProccessException(processor, args, message, ex);
            }
        }        

        private void ValidateConsumerStarted()
        {
            if (!string.IsNullOrEmpty(ConsumerTag))
                throw new InvalidOperationException(Resources.ConsumerAlreadyStarted);
        }
        
        private static void ValidateQueueName(string queue)
        {
            if (string.IsNullOrEmpty(queue))
                throw new ArgumentNullException(ParamName, Resources.QueueNameNotFound);
        }        
        
        private static void SetReplyQueue(BasicDeliverEventArgs args, IMessage message)
        {
            if (!string.IsNullOrEmpty(args.BasicProperties.ReplyTo))
                message.ReplyQueue = args.BasicProperties.ReplyTo;
        }
        
        private void ProccessException(IProcessor processor, BasicDeliverEventArgs args, IMessage message, Exception ex)
        {
            try
            {
                if (_messageProcessor.HandleProcessError(message, _publisher, ex, processor.HandleProcessError))
                    _channel.BasicAck(args.DeliveryTag, false);
                else
                    _channel.BasicNack(args.DeliveryTag, false, true);
            }
            catch
            {
                _channel.BasicNack(args.DeliveryTag, false, true);
            }
        }
    }
}
