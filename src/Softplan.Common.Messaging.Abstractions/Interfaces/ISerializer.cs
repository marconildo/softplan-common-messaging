using System;
using System.Text;

namespace Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces
{
    public interface ISerializer
    {
        string Serialize(IMessage message);

        byte[] Serialize(IMessage message, Encoding encoding);

        T Deserialize<T>(string data) where T : IMessage;

        IMessage Deserialize(Type messageType, string data);
    }
}
