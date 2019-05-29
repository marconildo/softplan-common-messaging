namespace Softplan.Common.Messaging.RabbitMq.Abstractions
{
    public interface IConsumer
    {
        void Start(IProcessor processor, string queue);
        void Stop();
    }
}
