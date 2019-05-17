using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using Softplan.Common.Messaging.Abstractions;
using Softplan.Common.Messaging.Properties;
using Softplan.Common.Messaging.Tests.TestProcessors;
using Xunit;

namespace Softplan.Common.Messaging.Tests
{
    public class MessagingManagerTest
    {
        private readonly Mock<IBuilder> _builderMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IConsumer> _consumerMock;
        private readonly Mock<ILogger> _loggerMock;

        public MessagingManagerTest()
        {
            _builderMock = new Mock<IBuilder>();            
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _consumerMock = new Mock<IConsumer>();
            _loggerMock = new Mock<ILogger>();
            _consumerMock.Setup(c => c.Start(It.IsAny<IProcessor>(), It.IsAny<string>()));
            _consumerMock.Setup(c => c.Stop());
            _builderMock.Setup(b => b.BuildConsumer()).Returns(_consumerMock.Object);
            var mockMap = new Mock<IDictionary<string, Type>>();
            _builderMock.SetupGet(b => b.MessageQueueMap).Returns(mockMap.Object);           
            _loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            _loggerMock.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }
        
        [Fact]
        public void When_Start_Should_Set_As_Active()
        {       
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            
            manager.Start();
            
            manager.Active.Should().Be(true);
        }
        
        [Fact]
        public void When_Start_Should_Log_Information_About_Initialization()
        {       
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            
            manager.Start();
            
            _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerStarting), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
            _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerStarted), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public void When_Start_Should_Build_Consumer_To_Enableds_Processors()
        {            
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());
            
            manager.Start();
            
            _builderMock.Verify(b => b.BuildConsumer(), Times.Exactly(manager.EnabledProcessors.Count));
        }
        
        [Fact]
        public void When_Start_Should_Start_Each_Created_Consumer()
        {            
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());
            
            manager.Start();
            
            _consumerMock.Verify(c => c.Start(It.IsAny<IProcessor>(), It.IsAny<string>()), Times.Exactly(manager.EnabledProcessors.Count));
        }
        
        [Fact]
        public void When_Start_And_Already_Active_Should_Log_Information()
        {    
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object){ Active = true};
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());
            
            manager.Start();
            
            _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerAlreadyStarted), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public void When_Start_And_Already_Active_Should_Not_Build_Consumer_To_Enableds_Processors()
        {            
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object){ Active = true};
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());
            
            manager.Start();
            
            _builderMock.Verify(b => b.BuildConsumer(), Times.Never);
        }
        
        [Fact]
        public void When_Start_With_Error_Should_Set_As_Inactive()
        {     
            _builderMock.Setup(b => b.BuildConsumer()).Throws<Exception>();
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());
            
            manager.Start();
            
            manager.Active.Should().Be(false);
        }
        
        [Fact]
        public void When_Start_With_Error_Should_Log_Information_About_Error()
        {    
            var exception = new Exception();
            _builderMock.Setup(b => b.BuildConsumer()).Throws(exception);
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());
            
            manager.Start();
            
            _loggerMock.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerErrorWhileStarting), exception, It.IsAny<Func<object, Exception, string>>()), Times.Once);            
        }
        
        [Fact]
        public void When_Stop_Should_Set_As_Inactive()
        {       
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object) {Active = true};
            
            manager.Stop();
            
            manager.Active.Should().Be(false);
        }
        
        [Fact]
        public void When_Stop_Should_Log_Information_About_Finalization()
        {       
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object) {Active = true};
            
            manager.Stop();
            
            _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerStopping), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
            _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerStopped), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public void When_Stop_Should_Stop_Each_Created_Consumer()
        {            
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());
            manager.Start();
            
            manager.Stop();
            
            _consumerMock.Verify(c => c.Stop(), Times.Exactly(manager.EnabledProcessors.Count));
        }
        
        [Fact]
        public void When_Stop_And_Inactive_Should_Log_Information()
        {    
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            
            manager.Stop();
            
            _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerNotStarted), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public void When_Stop_With_Error_Should_Log_Information_About_Error()
        {    
            var exception = new Exception(); 
            _consumerMock.Setup(c => c.Stop()).Throws(exception);
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            manager.EnabledProcessors.Add(GetProcessor<TestProcessor>());            
            manager.Start();            
            
            manager.Stop();
            
            _loggerMock.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), new FormattedLogValues(Properties.Resources.MQManagerErrorWhileStopping), exception, It.IsAny<Func<object, Exception, string>>()), Times.Once);            
        }

        [Fact]
        public void When_RegisterProcessor_Should_Add_Enabled_Processor()
        {            
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            var processor = new TestProcessor();
            manager.RegisterProcessor(processor);

            manager.EnabledProcessors.Count.Should().Be(1);
            manager.EnabledProcessors[0].Should().Be(processor);
        }
        
        [Fact]
        public void When_RegisterProcessor_Should_Add_MessageQueueMap_Item()
        {
            _builderMock.SetupGet(b => b.MessageQueueMap).Returns(new Dictionary<string, Type>());
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            var processor = new TestProcessor();
            manager.RegisterProcessor(processor);

            _builderMock.Object.MessageQueueMap[processor.GetQueueName()].Should().Be(processor.GetMessageType());
        }
        
        [Fact]
        public void When_LoadProcessor_Should_Load_Only_Valid_Processors()
        {
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            
            manager.LoadProcessors(new Mock<IServiceProvider>().Object);
            
            Assert.Equal(1, manager.EnabledProcessors.Count);
            Assert.IsType<TestProcessor>(manager.EnabledProcessors[0]);
        }
        
        [Fact]
        public void When_LoadProcessor_Should_Log_Invalid_Processors()
        {
            var processorIgnorerMock = new Mock<IProcessorIgnorer>();
            processorIgnorerMock.Setup(i => i.ShouldIgnoreProcessorFrom(It.IsAny<Type>()))
                .Returns((Type t) => t == typeof(TestProcessor));
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object)
            {
                ProcessorIgnorer = processorIgnorerMock.Object
            };
            
            manager.LoadProcessors(new Mock<IServiceProvider>().Object);
            
            _loggerMock.Verify(l => l.Log(LogLevel.Debug, It.IsAny<EventId>(), new FormattedLogValues(string.Format(Resources.ProcessorIsInvalid, typeof(AbstractTestProcessor))), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
            _loggerMock.Verify(l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), new FormattedLogValues(string.Format(Resources.ErrorToCreateProcessorInstance, typeof(InvalidConstructorProcessor))), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
            _loggerMock.Verify(l => l.Log(LogLevel.Debug, It.IsAny<EventId>(), new FormattedLogValues(string.Format(Resources.ProcessorWasIgnored, typeof(TestProcessor))), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public void When_LoadProcessor_Already_registered_Should_Log()
        {
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object);
            manager.RegisterProcessor(new TestProcessor());

            manager.LoadProcessors(new Mock<IServiceProvider>().Object);
            
            _loggerMock.Verify(l => l.Log(LogLevel.Debug, It.IsAny<EventId>(), new FormattedLogValues(string.Format(Resources.ProcessorAlreadyResgistered, typeof(TestProcessor))), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
        
        [Fact]
        public void DisposeTest()
        {
            var manager = new MessagingManager(_builderMock.Object, _loggerFactoryMock.Object) {Active = true};
            manager.Dispose();
        }
        
                

        private static IProcessor GetProcessor<T>() where T:IProcessor
        {
            return Builder<T>.CreateNew().Build();
        }
    }
}
