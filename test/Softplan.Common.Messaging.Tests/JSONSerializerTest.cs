using System.Text;
using Xunit;

namespace Softplan.Common.Messaging.Tests
{
    public class JsonSerializerTests
    {

        private const string simpleMsgJson = @"{""Id"":null,""Headers"":{},""OperationId"":""4"",""ParentOperationId"":""3"",""MainOperationId"":""2"",""CustomParams"":{""items"":[]},""UserId"":""1"",""Token"":null,""ReplyQueue"":null,""ReplyTo"":null}";
        [Fact]
        public void SerializeSimpleObjectTest()
        {
            var obj = new Message
            {
                UserId = "1",
                MainOperationId = "2",
                ParentOperationId = "3",
                OperationId = "4"
            };

            var serializer = new JsonSerializer();

            Assert.Equal(simpleMsgJson, serializer.Serialize(obj));
        }

        [Fact]
        public void SerializeToArrayTest()
        {
            var obj = new Message
            {
                UserId = "1",
                MainOperationId = "2",
                ParentOperationId = "3",
                OperationId = "4"
            };

            var serializer = new JsonSerializer();

            Assert.Equal(Encoding.UTF8.GetBytes(simpleMsgJson), serializer.Serialize(obj, Encoding.UTF8));
        }

        [Fact]
        public void DeSerializeSimpleObjectWithGenericsTest()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize<Message>(simpleMsgJson);
            Assert.Equal("1", obj.UserId);
            Assert.Equal("2", obj.MainOperationId);
            Assert.Equal("3", obj.ParentOperationId);
            Assert.Equal("4", obj.OperationId);
        }

        [Fact]
        public void DeSerializeSimpleObjectTest()
        {
            var serializer = new JsonSerializer();

            var obj = serializer.Deserialize(typeof(Message), simpleMsgJson);
            Assert.Equal("1", obj.UserId);
            Assert.Equal("2", obj.MainOperationId);
            Assert.Equal("3", obj.ParentOperationId);
            Assert.Equal("4", obj.OperationId);
        }
    }
}
