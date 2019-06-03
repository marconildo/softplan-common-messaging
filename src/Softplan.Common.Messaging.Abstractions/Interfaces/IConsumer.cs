namespace Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces
{
    public interface IConsumer
    {
        void Start(IProcessor processor, string queue);
        void Stop();
    }
}
