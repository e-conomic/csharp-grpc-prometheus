using System;
using NetGrpcPrometheus.Helpers;
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
            string callType = GrpcCallType.Unary;
            string methodName = _client.UnaryName;

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType, methodName));
        }

        [Test]
        public void Server_ClientStreaming()
        {
            string callType = GrpcCallType.ClientStreaming;
            string methodName = _client.ClientStreamingName;

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamReceivedCounter.Name, callType, methodName));
        }

        [Test]
        public void Server_ServerStreaming()
        {
            string callType = GrpcCallType.ServerStreaming;
            string methodName = _client.ServerStreamingName;

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamSentCounter.Name, callType, methodName));
        }

        [Test]
        public void Server_DuplexStreaming()
        {
            string callType = GrpcCallType.DuplexStreaming;
            string methodName = _client.DuplexStreamingName;

            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.RequestCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.ResponseCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.LatencyHistogram.Name + "_bucket", callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamReceivedCounter.Name, callType, methodName));
            Assert.IsTrue(Utils.ContainsMetric(TestServer.Metrics.StreamSentCounter.Name, callType, methodName));
        }
    }
}
