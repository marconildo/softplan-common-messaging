using FluentAssertions;
using Softplan.Common.Messaging.ElasticApm.Constants;
using Xunit;

namespace Softplan.Common.Messaging.ElasticApm.Tests.Constants
{
    public class ElasticApmConstantsTest
    {
        [Theory]
        [InlineData("TraceParent", "trace-parente")]
        [InlineData("TransactionName", "transaction-name")]
        public void When_Get_A_Property_Value_Should_Return_The_Expected(string propertyName, string expected)
        {
            var value = typeof(ElasticApmConstants).GetProperty(propertyName).GetValue(null, null);

            value.Should().Be(expected);
        }
    }
}