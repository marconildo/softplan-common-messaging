using System;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Abstractions;

namespace simplePubSub
{
    public class ExampleProcessor : IProcessor
    {
        public ILogger Logger { get; set; }

        public Type GetMessageType()
        {
            return typeof(ExampleMessage);
        }

        public string GetQueueName()
        {
            return "testQueue123";
        }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error)
        {
            Console.WriteLine("It failed :(");
            return true;
        }

        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            Console.WriteLine($"{((ExampleMessage)message).Text} - It works :-D !");
        }

    }
}
