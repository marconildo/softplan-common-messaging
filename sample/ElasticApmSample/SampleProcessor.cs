using System;
using System.Threading;
using ElasticApmsample.Properties;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace ElasticApmsample
{
    public class SampleProcessor : IProcessor
    {
        private const string QueueName = "ElasticApmQueue";
        
        public ILogger Logger { get; set; }

        public Type GetMessageType()
        {
            return typeof(SampleMessage);
        }

        public string GetQueueName()
        {            
            return QueueName;
        }        

        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            Thread.Sleep(10000);
            Console.WriteLine(Resources.MessageSuccessfullyProcessed, ((SampleMessage)message).Text);
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error)
        {
            Console.WriteLine(Resources.MessageHandlingError);
            return true;
        }
    }
}
