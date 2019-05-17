using System;
using Microsoft.Extensions.Logging;
using Softplan.Common.Messaging.Abstractions;

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

        private int CalculateFib(int number)
        {
            if (number == 0 || number == 1) return number;
            return CalculateFib(number - 1) + CalculateFib(number - 2);
        }
        public void ProcessMessage(IMessage message, IPublisher publisher)
        {
            var number = ((FibMessage)message).Number;
            if (number < 0)
                throw new ArgumentOutOfRangeException("Fibpnacci requer que o número seja maior que zero.");

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
    }
}
