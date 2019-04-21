namespace Softplan.Common.Messaging.Abstractions
{
    public interface IConsumer
    {
        void Start(IProcessor processor, string queue);
        void Stop();
    }
}
