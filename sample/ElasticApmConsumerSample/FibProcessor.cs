using System;
using System.Threading;
using ElasticApmConsumerSample.Properties;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace ElasticApmConsumerSample
{
    public class FibProcessor : IProcessor
    {
        private const string QueueName = "FibDestination";
        
        public ILogger Logger { get; set; }
        
        public string GetQueueName()
        {
            return QueueName;
        }

        public Type GetMessageType()
        {
            return typeof(FibMessage);
        }
        
        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            var randon = new Random();
            Thread.Sleep(randon.Next(500,1500));            
            var number = ((FibMessage)message).Number;
            if (number < 0)
                throw new ArgumentOutOfRangeException(Resources.InvalidFibValue);
            Console.WriteLine(Resources.MessageSuccessfullyProcessed, ((FibMessage)message).Number);
            publisher.Publish(new FibMessage(message) { Number = CalculateFib(number) });
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error)
        {
            publisher.Publish(new FibMessage(message) { ErrorMessage = error.Message });
            return true;
        }                                
        
        private static int CalculateFib(int number)
        {
            if (number == 0 || number == 1) return number;
            return CalculateFib(number - 1) + CalculateFib(number - 2);
        }
    }
}
