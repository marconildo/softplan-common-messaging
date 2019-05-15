using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using RabbitMQ.Client;
using Softplan.Common.Messaging.Infrastructure;
using Xunit;

namespace Softplan.Common.Messaging.Tests.Infrastructure
{
    public class RabbitMQApiManagerTest
    {
        private readonly Mock<IModel> channelMock;
        private Mock<HttpMessageHandler> httpHandlerMock;

        public RabbitMQApiManagerTest()
        {
            channelMock = new Mock<IModel>();
            channelMock.Setup(chan => chan.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                    .Returns((string queueName, bool durable, bool exclusive, bool autodelete, IDictionary<string, object> props) => new QueueDeclareOk(queueName, 0, 0));
        }

        private Task<HttpResponseMessage> GetHttpRequestHandler(HttpStatusCode responseCode, string responseBody)
        {
            return Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                return new HttpResponseMessage(responseCode)
                {
                    Content = new StringContent(responseBody)
                };
            });
        }

        private HttpClient GetMockHttpClient(Task<HttpResponseMessage> requestHandler, Action<HttpRequestMessage, CancellationToken> callback)
        {
            /*
                (r, c) =>
                {
                    Assert.AreEqual(HttpMethod.Get, r.Method);
                }
             */
            httpHandlerMock = new Mock<HttpMessageHandler>();

            httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(requestHandler)
                .Callback<HttpRequestMessage, CancellationToken>(callback);

            return new HttpClient(httpHandlerMock.Object);
        }

        [Fact]
        public void GetQueueInfoTest()
        {
            const string response = @"{""auto_delete"": false, ""exclusive"": false, ""durable"": true,
                ""arguments"": { ""x-max-priority"": 5 } }";

            void validationCallback(HttpRequestMessage req, CancellationToken ct)
            {
                Assert.Equal(req.Method, HttpMethod.Get);
                Assert.Equal("/api/queues/%2f/testQueueName?columns=durable,auto_delete,exclusive,arguments", req.RequestUri.PathAndQuery);
            }

            var manager = new RabbitMQApiManager("http://guest:guest@172.21.11.6:8080/api/", "guest", "guest", channelMock.Object,
                client: GetMockHttpClient(
                    requestHandler: GetHttpRequestHandler(HttpStatusCode.OK, response),
                    callback: validationCallback
                    )
                );

            var info = manager.GetQueueInfo("testQueueName");
            Assert.True(info.Durable);
            Assert.False(info.AutoDelete);
            Assert.False(info.Exclusive);
            Assert.True(info.Arguments.ContainsKey("x-max-priority"));
            Assert.Equal(5, info.Priority);
            httpHandlerMock.Protected().Verify("SendAsync", Times.Once(), new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() });

        }

        [Fact]
        public void GetNotExistentQueueInfoTest()
        {
            void validationCallback(HttpRequestMessage req, CancellationToken ct)
            {
                Assert.Equal(req.Method, HttpMethod.Get);
                Assert.Equal("/api/queues/%2f/amazing?columns=durable,auto_delete,exclusive,arguments", req.RequestUri.PathAndQuery);
            }

            var manager = new RabbitMQApiManager("http://guest:guest@172.21.11.6:8080/api", "guest", "guest", channelMock.Object,
                client: GetMockHttpClient(
                    requestHandler: GetHttpRequestHandler(HttpStatusCode.NotFound, String.Empty),
                    callback: validationCallback
                    )
                );

            var info = manager.GetQueueInfo("amazing");
            Assert.True(info.Durable);
            Assert.False(info.AutoDelete);
            Assert.False(info.Exclusive);
            Assert.Equal(0, info.Arguments.Count);
            Assert.Equal(0, info.Priority);

        }

        [Fact]
        public void GetUnauthorizedResponseTest()
        {
            void validationCallback(HttpRequestMessage req, CancellationToken ct)
            {
                Assert.Equal(req.Method, HttpMethod.Get);
                Assert.Equal("/api/queues/%2f/amazing?columns=durable,auto_delete,exclusive,arguments", req.RequestUri.PathAndQuery);
            }

            var manager = new RabbitMQApiManager("http://guest:guest@172.21.11.6:8080/api", "guest", "guest", channelMock.Object,
                client: GetMockHttpClient(
                    requestHandler: GetHttpRequestHandler(HttpStatusCode.Unauthorized, "This is an error!"),
                    callback: validationCallback
                    )
                );

            Exception ex = Assert.Throws<HttpRequestException>(() => manager.GetQueueInfo("amazing"));
            Assert.Equal("Erro ao consultar API do RabbitMQ. Status Unauthorized - This is an error!", ex.Message);
        }

        [Fact]
        public void EnsureAMQPQueueTest()
        {
            var manager = new RabbitMQApiManager("http://guest:guest@172.21.11.6:8080/api", "guest", "guest", channelMock.Object, null);

            Assert.Equal("amq.amazing", manager.EnsureQueue("amq.amazing"));
        }

        [Fact]
        public void EnsureQueueTest()
        {
            const string response = @"{""auto_delete"": false, ""exclusive"": false, ""durable"": true,
                ""arguments"": { ""x-max-priority"": 5 } }";

            void validationCallback(HttpRequestMessage req, CancellationToken ct)
            {
                Assert.Equal(req.Method, HttpMethod.Get);
                Assert.Equal("/api/queues/%2f/?columns=durable,auto_delete,exclusive,arguments", req.RequestUri.PathAndQuery);
            }

            var manager = new RabbitMQApiManager("http://guest:guest@172.21.11.6:8080/api/", "guest", "guest", channelMock.Object,
                client: GetMockHttpClient(
                    requestHandler: GetHttpRequestHandler(HttpStatusCode.OK, response),
                    callback: validationCallback
                    )
                );
            channelMock.Setup(c => c.QueueDeclare("", It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
               It.IsAny<IDictionary<string, object>>())).Returns(new QueueDeclareOk("autoGenQueue", 1, 1));

            Assert.Equal("autoGenQueue", manager.EnsureQueue(""));
            httpHandlerMock.Protected().Verify("SendAsync", Times.Once(), new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() });

        }
    }
}
