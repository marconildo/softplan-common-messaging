using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace ElasticApmsample
{
    public class SampleMessage : Message, IMessage
    {
        public string Text { get; set; }
    }
}
