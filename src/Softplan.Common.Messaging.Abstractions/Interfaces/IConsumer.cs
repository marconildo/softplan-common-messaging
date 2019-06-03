namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IConsumer
    {
        void Start(IProcessor processor, string queue);
        void Stop();
    }
}
