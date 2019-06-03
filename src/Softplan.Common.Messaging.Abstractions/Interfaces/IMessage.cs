using System.Collections.Generic;

namespace Softplan.Common.Messaging.Abstractions.Interfaces
{
    public interface IMessage
    {
        string Id { get; set; }
        IDictionary<string, object> Headers { get; set; }
        string OperationId { get; set; }
        string ParentOperationId { get; set; }
        string MainOperationId { get; set; }
        IDictionary<string, string> CustomParams { get; set; }
        string UserId { get; set; }
        string Token { get; set; }
        string ReplyQueue { get; set; }
        string ReplyTo { get; set; }

        void AssignBaseMessageData(IMessage baseMessage);
    }
}
