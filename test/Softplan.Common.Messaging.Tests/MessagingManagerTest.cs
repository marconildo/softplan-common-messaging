using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Tests.TestProcessors;
using Xunit;

namespace Softplan.Common.Messaging.Tests
{
    public class MessagingManagerTest
    {
        private readonly Mock<IBuilder> builderMock = new Mock<IBuilder>();
        private readonly Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

        public MessagingManagerTest()
        {
            loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);
        }

        [Fact]
        public void StartTest()
        {
            var manager = new MessagingManager(builderMock.Object, loggerFactoryMock.Object) {Active = true};
            Assert.True(manager.Active);
        }


        [Fact]
        public void StartTwiceTest()
        {
            var manager = new MessagingManager(builderMock.Object, loggerFactoryMock.Object) {Active = true};
            Assert.True(manager.Active);
            manager.Start();
            Assert.True(manager.Active);
        }

        [Fact]
        public void StopTest()
        {
            var manager = new MessagingManager(builderMock.Object, loggerFactoryMock.Object) {Active = true};
            Assert.True(manager.Active);
            manager.Active = false;
            Assert.False(manager.Active);
        }

        [Fact]
        public void StopTwiceTest()
        {
            var manager = new MessagingManager(builderMock.Object, loggerFactoryMock.Object) {Active = true};
            Assert.True(manager.Active);
            manager.Active = false;
            Assert.False(manager.Active);
            manager.Stop();
            Assert.False(manager.Active);
        }

        [Fact]
        public void DisposeTest()
        {
            var manager = new MessagingManager(builderMock.Object, loggerFactoryMock.Object) {Active = true};
            manager.Dispose();
        }

        [Fact]
        public void LoadProcessorTest()
        {
            var mockMap = new Mock<IDictionary<string, Type>>();
            builderMock.SetupGet(b => b.MessageQueueMap)
                .Returns(mockMap.Object);
            var processorIgnorerMock = new Mock<IProcessorIgnorer>();
            processorIgnorerMock.Setup(i => i.ShouldIgnoreProcessorFrom(It.IsAny<Type>()))
                .Returns((Type v) => v.Namespace.Contains("Castle.Proxies"));
            var manager = new MessagingManager(builderMock.Object, loggerFactoryMock.Object)
            {
                ProcessorIgnorer = processorIgnorerMock.Object
            };
            
            manager.LoadProcessors(new Mock<IServiceProvider>().Object);
            
            Assert.Equal(1, manager.EnabledProcessors.Count);
            Assert.IsType<TestProcessor>(manager.EnabledProcessors[0]);
        }

        [Fact]
        public void LoadAlreadyRegisteredProcessorTest()
        {
            var mockMap = new Mock<IDictionary<string, Type>>();
            builderMock.SetupGet(b => b.MessageQueueMap)
                .Returns(mockMap.Object);
            var processorIgnorerMock = new Mock<IProcessorIgnorer>();
            processorIgnorerMock.Setup(i => i.ShouldIgnoreProcessorFrom(It.IsAny<Type>()))
                .Returns((Type v) => v.Namespace.Contains("Castle.Proxies"));
            var manager = new MessagingManager(builderMock.Object, loggerFactoryMock.Object)
            {
                ProcessorIgnorer = processorIgnorerMock.Object
            };
            manager.RegisterProcessor(new TestProcessor());
            Assert.Equal(1, manager.EnabledProcessors.Count);

            manager.LoadProcessors(new Mock<IServiceProvider>().Object);
            
            Assert.Equal(1, manager.EnabledProcessors.Count);
            Assert.IsType<TestProcessor>(manager.EnabledProcessors[0]);
        }
    }
}
