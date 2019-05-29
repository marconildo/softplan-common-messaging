using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;

namespace rpcExample
{
    public class FibMessage : Message, IMessage
    {
        public FibMessage(IMessage parentMessage = null) : base(parentMessage)
        {
        }

        public int Number { get; set; }
        public string ErrorMessage { get; set; }
    }
}
