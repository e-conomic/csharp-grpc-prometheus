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

        private static readonly string GrpcCodeOk = StatusCode.OK.ToString();
        private static readonly string GrpcCodeInternal = StatusCode.Internal.ToString();
        private static readonly string UnaryTypeName = "unary";
        private static readonly string ClientStreamingTypeName = "client_stream";
        private static readonly string ServerStreamingTypeName = "server_stream";
        private static readonly string DuplexStreamingTypeName = "bidi_stream";

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
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, UnaryTypeName, _client.UnaryName,
                null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, UnaryTypeName,
                _client.UnaryName, GrpcCodeOk,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", UnaryTypeName,
                _client.UnaryName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Server_ClientStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, GrpcCodeOk,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket",
                ClientStreamingTypeName,
                _client.ClientStreamingName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamReceivedCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Server_ServerStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, GrpcCodeOk,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket",
                ServerStreamingTypeName,
                _client.ServerStreamingName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamSentCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Server_DuplexStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, GrpcCodeOk,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket",
                DuplexStreamingTypeName,
                _client.DuplexStreamingName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamReceivedCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamSentCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
        }

        [Test]
        public void Server_Failing_Unary()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, UnaryTypeName, _client.UnaryName,
                null,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, UnaryTypeName,
                _client.UnaryName, GrpcCodeInternal,
                TestServer.MetricsHostname, TestServer.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", UnaryTypeName,
                _client.UnaryName, null, TestServer.MetricsHostname, TestServer.MetricsPort));
        }
        
        [Test]
        public void Client_Unary()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, UnaryTypeName, _client.UnaryName,
                null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, UnaryTypeName,
                _client.UnaryName, GrpcCodeOk,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket", UnaryTypeName,
                _client.UnaryName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_ClientStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, GrpcCodeOk,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket",
                ClientStreamingTypeName,
                _client.ClientStreamingName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_ServerStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, GrpcCodeOk,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket",
                ServerStreamingTypeName,
                _client.ServerStreamingName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_DuplexStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, GrpcCodeOk,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.LatencyHistogram.Name + "_bucket",
                DuplexStreamingTypeName,
                _client.DuplexStreamingName, null, TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_Unary()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, UnaryTypeName, _client.UnaryName,
                null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, UnaryTypeName,
                _client.UnaryName, GrpcCodeInternal,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_ClientStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, GrpcCodeInternal,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, ClientStreamingTypeName,
                _client.ClientStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_ServerStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, ServerStreamingTypeName,
                _client.ServerStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }

        [Test]
        public void Client_Failing_DuplexStreaming()
        {
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.RequestCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.ResponseCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamReceivedCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
            Assert.IsTrue(Utils.ContainsMetric(TestClient.Metrics.StreamSentCounter.Name, DuplexStreamingTypeName,
                _client.DuplexStreamingName, null,
                TestClient.MetricsHostname, TestClient.MetricsPort));
        }
    }
}
