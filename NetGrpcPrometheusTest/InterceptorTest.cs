using Grpc.Core;
using NetGrpcPrometheusTest.Helpers;
using NUnit.Framework;

namespace NetGrpcPrometheusTest
{
    [TestFixture]
    public class InterceptorTest
    {
        private TestServer _server;
        private TestClient _client;

        [OneTimeSetUp]
        public void SetUp()
        {
            _server = new TestServer();
            _client = new TestClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _server.Shutdown();
        }

        [Test]
        public void Server_Unary()
        {
            string callType = "unary";
            string methodName = _client.UnaryName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Server_ClientStreaming()
        {
            string callType = "client_stream";
            string methodName = _client.ClientStreamingName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamReceivedCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Server_ServerStreaming()
        {
            string callType = "server_stream";
            string methodName = _client.ServerStreamingName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamSentCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Server_DuplexStreaming()
        {
            string callType = "bidi_stream";
            string methodName = _client.DuplexStreamingName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamReceivedCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamSentCounter.Name, callType, methodName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Client_Unary()
        {
            string callType = "unary";
            string methodName = _client.UnaryName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_ClientStreaming()
        {
            string callType = "client_stream";
            string methodName = _client.ClientStreamingName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_ServerStreaming()
        {
            string callType = "server_stream";
            string methodName = _client.ServerStreamingName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_DuplexStreaming()
        {
            string callType = "bidi_stream";
            string methodName = _client.DuplexStreamingName;
            string grpcCode = StatusCode.OK.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket", callType,
                methodName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_Unary()
        {
            string callType = "unary";
            string methodName = _client.UnaryName;
            string grpcCode = StatusCode.Unavailable.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_ClientStreaming()
        {
            string callType = "client_stream";
            string methodName = _client.ClientStreamingName;
            string grpcCode = StatusCode.Unavailable.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, grpcCode,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_ServerStreaming()
        {
            string callType = "server_stream";
            string methodName = _client.ServerStreamingName;
            // TODO: when response status code on server streaming will be resolved this should be used
            // string grpcCode = StatusCode.Unavailable.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_DuplexStreaming()
        {
            string callType = "bidi_stream";
            string methodName = _client.DuplexStreamingName;
            // TODO: when response status code on server streaming will be resolved this should be used
            // string grpcCode = StatusCode.Unavailable.ToString();

            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, callType, methodName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }
    }
}
