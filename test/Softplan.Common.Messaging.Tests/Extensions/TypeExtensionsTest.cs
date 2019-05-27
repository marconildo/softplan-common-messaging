using System;
using FluentAssertions;
using Softplan.Common.Messaging.Extensions;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Extensions
{
    public class TypeExtensionsTest
    {
        [Theory]
        [InlineData(typeof(TestClass), true)]
        [InlineData(typeof(OtherTestClass), false)]
        public void When_Call_Implements_Should_Verify(Type myClass, bool expected)
        {
            var result = myClass.Implements<ITestInterface>();

            result.Should().Be(expected);
        }
        
        private interface ITestInterface
        {
            
        }
        
        private class TestClass : ITestInterface
        {
            
        }
        
        private class OtherTestClass
        {
            
        }
    }
}