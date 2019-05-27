using System.Text;
using FluentAssertions;
using Softplan.Common.Messaging.Abstractions;
using Xunit;

namespace Softplan.Common.Messaging.Tests
{
    public class MessageSerializerTests
    {
        private const string SimpleMsgJson = @"{""Id"":null,""Headers"":{},""OperationId"":""4"",""ParentOperationId"":""3"",""MainOperationId"":""2"",""CustomParams"":{""items"":[]},""UserId"":""1"",""Token"":null,""ReplyQueue"":null,""ReplyTo"":null}";
        private readonly MessageSerializer _serializer;

        public MessageSerializerTests()
        {
            _serializer = new MessageSerializer();
        }
        
        [Fact]
        public void When_Serialize_To_Json_Should_Return_Expected_String()
        {
            var msg = GetMessage();

            var jsonMessage = _serializer.Serialize(msg);

            jsonMessage.Should().Be(SimpleMsgJson);
        }        

        [Fact]
        public void When_Serialize_To_Byte_Array_Should_Return_Expected_Array()
        {
            var expected = Encoding.UTF8.GetBytes(SimpleMsgJson);
            var msg = GetMessage();

            var byteArray = _serializer.Serialize(msg, Encoding.UTF8);

            byteArray.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void When_Deserialize_With_Generic_Should_Return_expected_Object()
        {
            var msg = _serializer.Deserialize<Message>(SimpleMsgJson);

            msg.Should().BeOfType<Message>();
            ValidateMessage(msg);
        }

        [Fact]
        public void When_Deserialize_Simple_Object_Should_Return_An_IMessage()
        {
            var msg = _serializer.Deserialize(typeof(Message), SimpleMsgJson);

            msg.Should().BeAssignableTo<IMessage>();
            ValidateMessage(msg);
        }        


        private static Message GetMessage()
        {
            var obj = new Message
            {
                UserId = "1",
                MainOperationId = "2",
                ParentOperationId = "3",
                OperationId = "4"
            };
            return obj;
        }
        
        private static void ValidateMessage(IMessage msg)
        {
            Assert.Equal("1", msg.UserId);
            Assert.Equal("2", msg.MainOperationId);
            Assert.Equal("3", msg.ParentOperationId);
            Assert.Equal("4", msg.OperationId);
        }
    }
}
