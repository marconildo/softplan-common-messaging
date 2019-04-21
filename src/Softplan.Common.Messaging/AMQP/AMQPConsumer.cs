using System;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Abstractions;
using RabbitMQ.Client.Events;
using System.Text;

namespace Softplan.Common.Messaging.AMQP
{
    public class AmqpConsumer : IConsumer
    {
        private readonly IModel channel;
        private readonly IPublisher publisher;
        private readonly IBuilder builder;
        private readonly IQueueApiManager manager;
        public string ConsumerTag { get; private set; }

        public AmqpConsumer(IModel channel, IPublisher publisher, IBuilder builder, IQueueApiManager manager)
        {
            this.channel = channel ?? throw new System.ArgumentNullException(nameof(channel));
            this.publisher = publisher ?? throw new System.ArgumentNullException(nameof(publisher));
            this.builder = builder ?? throw new System.ArgumentNullException(nameof(builder));
            this.manager = manager ?? throw new System.ArgumentNullException(nameof(manager));
        }

        public void Start(IProcessor processor, string queue)
        {
            if (!string.IsNullOrEmpty(ConsumerTag))
                throw new InvalidOperationException("Consumer já iniciado.");

            if (string.IsNullOrEmpty(queue))
                throw new ArgumentNullException("queue","Nome da fila não informado");

            manager.EnsureQueue(queue);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (channel, args) => OnMessageReceived(processor, queue, args);

            ConsumerTag = channel.BasicConsume(
                queue: queue,
                autoAck: false,
                consumer: consumer
            );
        }

        protected void OnMessageReceived(IProcessor processor, string queue, BasicDeliverEventArgs args)
        {
            var message = builder.BuildMessage(queue, 1, Encoding.UTF8.GetString(args.Body));
            try
            {
                if (!String.IsNullOrEmpty(args.BasicProperties.ReplyTo))
                    message.ReplyQueue = args.BasicProperties.ReplyTo;

                processor.ProcessMessage(message, publisher);
                channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception err)
            {
                try
                {
                    if (processor.HandleProcessError(message, publisher, err))
                        channel.BasicAck(args.DeliveryTag, false);
                    else
                        channel.BasicNack(args.DeliveryTag, false, true);
                }
                catch
                {
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
            }

        }

        public void Stop()
        {
            if (!String.IsNullOrEmpty(ConsumerTag))
                channel.BasicCancel(ConsumerTag);

            ConsumerTag = string.Empty;
        }


    }
}
