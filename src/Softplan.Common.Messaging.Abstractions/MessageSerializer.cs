using System;
using System.Text;
using Newtonsoft.Json;
using Softplan.Common.Messaging.Abstractions.Interfaces;

namespace Softplan.Common.Messaging.Abstractions
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
