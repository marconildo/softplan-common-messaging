using Newtonsoft.Json;
using Softplan.Common.Messaging.Infrastructure;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Infrastructure
{
    public class QueueInfoTest
    {
        const string queueInfoWithPriority = "{\"Durable\":false,\"AutoDelete\":false,\"Exclusive\":false,"
            + "\"Arguments\":{\"x-max-priority\":99}}";
        const string queueInfoWithoutPriority = "{\"Durable\":false,\"AutoDelete\":false,\"Exclusive\":false}";

        [Fact]
        public void SerializeObject()
        {
            var info = new QueueInfo
            {
                Priority = 99
            };
            var serialized = JsonConvert.SerializeObject(info);
            Assert.Equal(queueInfoWithPriority, serialized);
        }

        [Fact]
        public void DeserializeObject()
        {
            var info = JsonConvert.DeserializeObject<QueueInfo>(queueInfoWithPriority);
            Assert.Equal(99, info.Priority);
        }

        [Fact]
        public void DeserializeObjectWithoutPriority()
        {
            var info = JsonConvert.DeserializeObject<QueueInfo>(queueInfoWithoutPriority);
            Assert.Equal(0, info.Priority);
        }
    }
}
