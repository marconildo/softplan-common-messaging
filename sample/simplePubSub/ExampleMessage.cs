using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;

namespace simplePubSub
{
    public class ExampleMessage : Message, IMessage
    {
        public string Text { get; set; }
    }
}
