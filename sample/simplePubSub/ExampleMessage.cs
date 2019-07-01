using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace simplePubSub
{
    public class ExampleMessage : Message, IMessage
    {
        public string Text { get; set; }
    }
}
