using System;
using Microsoft.Extensions.Logging;
using rpcExample.Properties;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace rpcExample
{
    public class FibProcessor : IProcessor
    {
        public ILogger Logger { get; set; }

        public bool HandleProcessError(IMessage message, IPublisher publisher, Exception error)
        {
            publisher.Publish(new FibMessage(message) { ErrorMessage = error.Message });
            return true;
        }

        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            var number = ((FibMessage)message).Number;
            if (number < 0)
                throw new ArgumentOutOfRangeException(Resources.NumeroMenorQueZero);

            publisher.Publish(new FibMessage(message) { Number = CalculateFib(number) });
        }

        public string GetQueueName()
        {
            return "test.fibonacci";
        }

        public Type GetMessageType()
        {
            return typeof(FibMessage);
        }
        public static int CalculateFib(int number)
        {
            int a = 0;
            int b = 1;
            // In number steps compute Fibonacci sequence iteratively.
            for (int i = 0; i < number; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

    }
}




