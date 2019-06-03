using System;
using Moq;
using Softplan.Common.Messaging.Extensions;
using Softplan.Common.Messaging.RabbitMq.Abstractions.Interfaces;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Extensions
{
    public class ServiceProviderExtensionsTest
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IMessagingManager> _messagingManager;

        public ServiceProviderExtensionsTest()
        {
            const MockBehavior mockBehavior = MockBehavior.Strict;
            _serviceProviderMock = new Mock<IServiceProvider>(mockBehavior);
            _messagingManager = new Mock<IMessagingManager>(mockBehavior);
            _serviceProviderMock.Setup(s => s.GetService(typeof(IMessagingManager))).Returns(_messagingManager.Object);
            _messagingManager.Setup(m => m.LoadProcessors(It.IsAny<IServiceProvider>()));
            _messagingManager.Setup(m => m.Start());
        }

        [Fact]
        public void When_Call_StartMessagingManager_Should_Get_IMessagingManager()
        {
            ServiceProviderExtensions.StartMessagingManager(_serviceProviderMock.Object);

            _serviceProviderMock.Verify(s => s.GetService(typeof(IMessagingManager)), Times.Once);
        }
        
        [Fact]
        public void When_Call_StartMessagingManager_Should_Load_Processors()
        {
            ServiceProviderExtensions.StartMessagingManager(_serviceProviderMock.Object);

            _messagingManager.Verify(m => m.LoadProcessors(It.IsAny<IServiceProvider>()), Times.Once);
        }
        
        [Fact]
        public void When_Call_StartMessagingManager_Should_Start_Messaging_Manager()
        {
            ServiceProviderExtensions.StartMessagingManager(_serviceProviderMock.Object);

            _messagingManager.Verify(m => m.Start(), Times.Once);
        }
    }
}