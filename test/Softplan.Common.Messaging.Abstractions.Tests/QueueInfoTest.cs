using FluentAssertions;
using Newtonsoft.Json;
using Softplan.Common.Messaging.RabbitMq.Abstractions;
using Xunit;

namespace Softplan.Common.Messaging.Abstractions.Tests
{
    public class QueueInfoTest
    {
        private const string QueueInfoWithPriority = "{\"Durable\":false,\"AutoDelete\":false,\"Exclusive\":false,\"Arguments\":{\"x-max-priority\":99}}";
        private const string QueueInfoWithoutPriority = "{\"Durable\":false,\"AutoDelete\":false,\"Exclusive\":false}";

        [Fact]
        public void When_Serialize_Should_Add_Arguments_Max_Priority()
        {
            var info = new QueueInfo
            {
                Priority = 99
            };
            
            var serialized = JsonConvert.SerializeObject(info);
            
            serialized.Should().Be(QueueInfoWithPriority);
        }

        [Fact]
        public void When_Deserialize_With_Priority_Should_Set_Value()
        {
            var info = JsonConvert.DeserializeObject<QueueInfo>(QueueInfoWithPriority);

            info.Priority.Should().Be(99);
        }

        [Fact]
        public void When_Deserialize_Without_Priority_Should_Set_To_Zero()
        {
            var info = JsonConvert.DeserializeObject<QueueInfo>(QueueInfoWithoutPriority);
            
            info.Priority.Should().Be(0);
        }
    }
}
