using FluentAssertions;
using Softplan.Common.Messaging.Abstractions.Constants;
using Xunit;

namespace Softplan.Common.Messaging.Abstractions.Tests.Constants
{
    public class ApmConstantsTest
    {
        [Theory]
        [InlineData("TraceParent", "trace-parente")]
        [InlineData("TransactionName", "transaction-name")]
        [InlineData("ApmTraceAsyncTransaction", "Apm-Trace-Async-Transaction")]
        public void When_Get_A_Property_Value_Should_Return_The_Expected(string propertyName, string expected)
        {
            var value = typeof(ApmConstants).GetProperty(propertyName).GetValue(null, null);

            value.Should().Be(expected);
        }
    }
}