using System;
using System.Threading.Tasks;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IMessagePublisher
    {
        void Publish(IMessage message, string destination, bool forceDestination, Action<IMessage, string, bool> publish);
        Task<T> PublishAndWait<T>(IMessage message, string destination, bool forceDestination, int milliSecondsTimeout, Func<IMessage, string, bool, int, Task<T>> publishAndWait) where T : IMessage;
    }
}