using Softplan.Common.Messaging;
using Softplan.Common.Messaging.Abstractions;

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
