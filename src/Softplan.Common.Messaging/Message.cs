using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Softplan.Common.Messaging.Abstractions;

namespace Softplan.Common.Messaging
{
    public class Message : IMessage
    {
        public string Id { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public string OperationId { get; set; }
        public string ParentOperationId { get; set; }
        public string MainOperationId { get; set; }
        [JsonProperty("CustomParams")]
        public LegacyCustomParams LegacyCustomParams { get; set; }
        [JsonIgnore]
        public IDictionary<string, string> CustomParams { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string ReplyQueue { get; set; }
        public string ReplyTo { get; set; }
        
        public Message(IMessage parentMessage = null)
        {
            OperationId = Guid.NewGuid().ToString();
            ParentOperationId = OperationId;
            MainOperationId = OperationId;
            CustomParams = new Dictionary<string, string>();
            LegacyCustomParams = new LegacyCustomParams();
            Headers = new Dictionary<string, object>();

            if (parentMessage != null)
            {
                AssignBaseMessageData(parentMessage);
            }
        }
        
        public void AssignBaseMessageData(IMessage baseMessage)
        {
            MainOperationId = baseMessage.MainOperationId;
            ParentOperationId = baseMessage.OperationId;
            ReplyQueue = baseMessage.ReplyQueue;
            foreach (var p in baseMessage.CustomParams)
            {
                CustomParams.Add(p.Key, p.Value);
            }
        }

        [OnSerializing()]
        private void OnSerializing(StreamingContext context)
        {
            LegacyCustomParams.FromDictionary(CustomParams);
        }

        [OnDeserialized()]
        private void OnDeSerialized(StreamingContext context)
        {
            LegacyCustomParams.ToDictionary(CustomParams);
        }
    }
}
