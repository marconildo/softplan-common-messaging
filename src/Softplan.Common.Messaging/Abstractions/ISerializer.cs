using System;
using System.Text;

namespace Softplan.Common.Messaging.Abstractions
{
    public interface ISerializer
    {
        string Serialize(IMessage message);

        byte[] Serialize(IMessage message, Encoding encoding);

        T Deserialize<T>(string data);

        IMessage Deserialize(Type messageType, string data);
    }
}
