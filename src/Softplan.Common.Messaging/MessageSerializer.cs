using System;
using System.Text;
using Softplan.Common.Messaging.Abstractions;
using Newtonsoft.Json;

namespace Softplan.Common.Messaging
{
    public class MessageSerializer : ISerializer
    {
        public T Deserialize<T>(string data) where T : IMessage
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public IMessage Deserialize(Type messageType, string data)
        {
            return (IMessage)JsonConvert.DeserializeObject(data, messageType);
        }

        public string Serialize(IMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public byte[] Serialize(IMessage message, Encoding encoding)
        {
            return encoding.GetBytes(Serialize(message));
        }
    }
}
