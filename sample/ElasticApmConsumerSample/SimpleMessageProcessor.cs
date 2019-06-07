using System;
using System.Threading;
using ElasticApmConsumerSample.Properties;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace ElasticApmConsumerSample
{
    public class SimpleMessageProcessor : IProcessor
    {
        private const string QueueName = "SimpleMessageDestination";
        
        public ILogger Logger { get; set; }

        public Type GetMessageType()
        {
            return typeof(SimpleMessage);
        }

        public string GetQueueName()
        {            
            return QueueName;
        }        

        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            var randon = new Random();
            Thread.Sleep(randon.Next(500,1500));
            Console.WriteLine(Resources.MessageSuccessfullyProcessed, ((SimpleMessage)message).Text);
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error)
        {
            Console.WriteLine(Resources.MessageHandlingError);
            return true;
        }
    }
}
