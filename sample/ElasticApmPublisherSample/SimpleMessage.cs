using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace ElasticApmPublisherSample
{
    public class SimpleMessage : Message, IMessage
    {
        public string Text { get; set; }
    }
}
