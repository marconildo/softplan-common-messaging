using Softplan.Common.Messaging.Abstractions;

namespace Softplan.Common.Messaging.Messages
{
    public class ErrorMessage : Message
    {
        public ErrorMessage(IMessage parentMessage = null) : base(parentMessage)
        {
        }

        public string Message { get; set; }
        public IMessage OriginalMessage { get; set; }
        public string OriginalQueue { get; set; }
    }
}
