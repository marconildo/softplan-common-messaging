using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Infrastructure;
using Softplan.Common.Messaging.Tests.Properties;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Infrastructure
{
    public class RabbitMqApiManagerTest
    {
        private readonly Mock<IModel> _channelMock;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private readonly MockBehavior _mockBehavior;
        
        private const string SendAsync = "SendAsync";
        private const string QueueName = "testQueueName";
        private const string AutoGenQueueName = "autoGenQueue";
        private const string Guest = "guest";
        private const string XMaxPriority = "x-max-priority";
        private const string ErrorMessage = "This is an error!";
        private const string Response = @"{""auto_delete"": false, ""exclusive"": false, ""durable"": true, ""arguments"": { ""x-max-priority"": 5 } }";
        private const string RabbitMqUrl = "http://guest:guest@172.21.11.6:8080/api";
        private const string Query = "/api/queues/%2f/{0}?columns=durable,auto_delete,exclusive,arguments";

        public RabbitMqApiManagerTest()
        {
            _mockBehavior = MockBehavior.Strict;
            _channelMock = new Mock<IModel>(_mockBehavior);
            _channelMock.Setup(chan => chan.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                    .Returns((string queueName, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> props) => new QueueDeclareOk(queueName, 0, 0));
        } 
        
        [Fact]
        public void When_GetQueueInfo_Should_Do_A_Request()
        {
            var manager = GetRabbitMqApiManager(HttpStatusCode.OK, Response, QueueName);

            manager.GetQueueInfo(QueueName);
            
            _httpHandlerMock.Protected().Verify(SendAsync, Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public void When_GetQueueInfo_With_Success_Should_Return_The_Expected_Data()
        {
            var expected = GetQueueInfo();
            expected.Priority = 5;
            expected.Arguments.Add(XMaxPriority, 5);            
            var manager = GetRabbitMqApiManager(HttpStatusCode.OK, Response, QueueName);

            var info = manager.GetQueueInfo(QueueName);
            
            info.Should().BeEquivalentTo(expected);
        }        

        [Fact]
        public void When_GetQueueInfo_And_Get_Not_Found_Should_Return_Default_Data()
        {
            var expected = GetQueueInfo();
            expected.Priority = 0;
            var manager = GetRabbitMqApiManager(HttpStatusCode.NotFound, string.Empty, QueueName);            
            
            var info = manager.GetQueueInfo(QueueName);
            
            info.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void When_GetQueueInfo_And_Get_Other_Result_Then_Success_Or_Not_Found_Should_Throws_Exception()
        {
            var manager = GetRabbitMqApiManager(HttpStatusCode.Unauthorized, ErrorMessage, QueueName);

            Action action = () =>  manager.GetQueueInfo(QueueName);
            
            action.Should()
                .Throw<HttpRequestException>()
                .WithMessage(string.Format(Resources.RabbitMQRequestError, HttpStatusCode.Unauthorized, ErrorMessage));
        }

        [Fact]
        public void When_EnsureQueue_And_Name_Match_Pattern_Should_Return_Name()
        {
            var queue = $"amq.{QueueName}";
            var manager = GetRabbitMqApiManager(HttpStatusCode.OK, Response, QueueName);            

            var result = manager.EnsureQueue(queue);

            result.Should().Be(queue);
        }

        [Fact]
        public void When_EnsureQueue_And_Name_Not_Match_Pattern_Should_Request_Queue_Info()
        {            
            var manager = GetRabbitMqApiManager(HttpStatusCode.OK, Response, QueueName);            
            _channelMock.Setup(c => c.QueueDeclare(QueueName, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
               It.IsAny<IDictionary<string, object>>())).Returns(new QueueDeclareOk(AutoGenQueueName, 1, 1));

            manager.EnsureQueue(QueueName);

           _httpHandlerMock.Protected().Verify(SendAsync, Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
        
        [Fact]
        public void When_EnsureQueue_And_Name_Not_Match_Pattern_And_Occurs_Error_When_Get_Queue_Info_Should_Propagate_Error()
        {            
            var manager = GetRabbitMqApiManager(HttpStatusCode.Forbidden, ErrorMessage, QueueName);          

            Action action = () =>  manager.EnsureQueue(QueueName);
            
            action.Should()
                .Throw<HttpRequestException>()
                .WithMessage(string.Format(Resources.RabbitMQRequestError, HttpStatusCode.Forbidden, ErrorMessage));
        }
        
        [Fact]
        public void When_EnsureQueue_And_Name_Not_Match_Pattern_Should_Declare_A_Queue()
        {
            var manager = GetRabbitMqApiManager(HttpStatusCode.OK, Response, QueueName);            
            _channelMock.Setup(c => c.QueueDeclare(QueueName, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>())).Returns(new QueueDeclareOk(AutoGenQueueName, 1, 1));

            manager.EnsureQueue(QueueName);

            _channelMock.Verify(c => c.QueueDeclare(QueueName, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),It.IsAny<IDictionary<string, object>>()), Times.Once);
        }
        
        [Fact]
        public void When_EnsureQueue_And_Name_Not_Match_Pattern_And_Occurs_Error_When_Declare_Queue_Should_Propagate_Error()
        {            
            var manager = GetRabbitMqApiManager(HttpStatusCode.OK, Response, QueueName);            
            _channelMock.Setup(c => c.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>())).Throws(new Exception(ErrorMessage));

            Action action = () =>  manager.EnsureQueue(QueueName);
            
            action.Should()
                .Throw<Exception>()
                .WithMessage(ErrorMessage);
        }
        
        [Fact]
        public void When_EnsureQueue_And_Name_Not_Match_And_No_Errors_Should_Return_Expected_Name()
        {            
            var manager = GetRabbitMqApiManager(HttpStatusCode.OK, Response, QueueName);            
            _channelMock.Setup(c => c.QueueDeclare(QueueName, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>())).Returns(new QueueDeclareOk(AutoGenQueueName, 1, 1));

            var result = manager.EnsureQueue(QueueName);

            result.Should().Be(AutoGenQueueName);
        }
        
        
        private static Task<HttpResponseMessage> GetHttpRequestHandler(HttpStatusCode responseCode, string responseBody)
        {
            return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(responseCode)
            {
                Content = new StringContent(responseBody)
            });
        }

        private HttpClient GetMockHttpClient(Task<HttpResponseMessage> requestHandler, Action<HttpRequestMessage, CancellationToken> callback)
        {
            _httpHandlerMock = new Mock<HttpMessageHandler>(_mockBehavior);            
            _httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(SendAsync, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(requestHandler)
                .Callback(callback);

            return new HttpClient(_httpHandlerMock.Object);
        }
        
        private RabbitMqApiManager GetRabbitMqApiManager(HttpStatusCode status, string response, string queue)
        {
            void ValidationCallback(HttpRequestMessage req, CancellationToken ct)
            {
                Assert.Equal(req.Method, HttpMethod.Get);
                Assert.Equal(string.Format(Query, queue), req.RequestUri.PathAndQuery);
            }

            var manager = new RabbitMqApiManager(RabbitMqUrl, Guest, Guest,
                _channelMock.Object,
                GetMockHttpClient(
                    GetHttpRequestHandler(status, response),
                    ValidationCallback
                )
            );
            return manager;
        }
        
        private static QueueInfo GetQueueInfo()
        {
            var expected = new QueueInfo
            {
                Durable = true,
                AutoDelete = false,
                Exclusive = false
            };
            return expected;
        }
    }
}
