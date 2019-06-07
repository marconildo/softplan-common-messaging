using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace ElasticApmConsumerSample
{
    public class SimpleMessage : Message, IMessage
    {
        public string Text { get; set; }
    }
}
