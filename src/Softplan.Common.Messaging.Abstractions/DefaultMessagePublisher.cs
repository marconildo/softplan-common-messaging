using System;
using System.Threading.Tasks;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.Abstractions
{
    public class DefaultMessagePublisher : IMessagePublisher
    {
        public void Publish(IMessage message, string destination, bool forceDestination, Action<IMessage, string, bool> publish)
        {
            publish(message, destination, forceDestination);
        }

        public Task<T> PublishAndWait<T>(IMessage message, string destination, bool forceDestination, int milliSecondsTimeout,
            Func<IMessage, string, bool, int, Task<T>> publishAndWait) where T : IMessage
        {
            return publishAndWait(message, destination, forceDestination, milliSecondsTimeout);
        }
    }
}