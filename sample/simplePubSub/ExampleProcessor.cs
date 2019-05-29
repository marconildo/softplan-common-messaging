using System;
using Microsoft.Extensions.Logging;
using rpcExample.Properties;
using Softplan.Common.Messaging.RabbitMq.Abstractions;

namespace simplePubSub
{
    public class ExampleProcessor : IProcessor
    {
        private const string QueueName = "testQueue123";
        
        public ILogger Logger { get; set; }

        public Type GetMessageType()
        {
            return typeof(ExampleMessage);
        }

        public string GetQueueName()
        {            
            return QueueName;
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error)
        {
            Console.WriteLine(Resources.Falha);
            return true;
        }

        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            Console.WriteLine(Resources.Sucesso, ((ExampleMessage)message).Text);
        }

    }
}
