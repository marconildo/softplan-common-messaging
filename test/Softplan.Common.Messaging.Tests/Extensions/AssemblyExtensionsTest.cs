using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Extensions
{
    public class AssemblyExtensionsTest
    {
        private Mock<Assembly> _assemblyMock;
        private const string ErrorMessage = "Value cannot be null.\nParameter name: assembly";

        public AssemblyExtensionsTest()
        {
            _assemblyMock = new Mock<Assembly>();
            var types = new[]
            {
                typeof(TestClass),
                typeof(OtherTestClass)
            };
            _assemblyMock.Setup(a => a.GetTypes()).Returns(types);
        }

        [Fact]
        public void When_Get_List_Implementatio_Of_Type_Should_Return_Expected_Values()
        {
            var types = Softplan.Common.Messaging.Extensions.AssemblyExtensions.ListImplementationsOf<ITestInterface>(_assemblyMock.Object).ToList();

            types.Should().HaveCount(1);
            types.Should().Contain(typeof(TestClass));
        }
        
        [Fact]
        public void When_Get_List_Implementatio_Of_Type_With_Null_assembly_Should_Return_ArgumentNullException()
        {
            Action action = () => Softplan.Common.Messaging.Extensions.AssemblyExtensions.ListImplementationsOf<ITestInterface>(null);

            action.Should()
                .Throw<ArgumentNullException>()
                .WithMessage(ErrorMessage);
        }
        
        [Fact]
        public void When_()
        {
            _assemblyMock.Setup(a => a.GetTypes()).Throws(new ReflectionTypeLoadException(new []{typeof(TestClass)}, new []{new Exception() }));
            var types = Softplan.Common.Messaging.Extensions.AssemblyExtensions.ListImplementationsOf<ITestInterface>(_assemblyMock.Object).ToList();

            types.Should().HaveCount(1);
            types.Should().Contain(typeof(TestClass));
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