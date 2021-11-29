using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NetGrpcPrometheus.Models;
using NetGrpcPrometheusTest.Helpers;
using NetGrpcPrometheusTest.Utils;
using NUnit.Framework;
using Status = NetGrpcPrometheusTest.Grpc.Status;

namespace NetGrpcPrometheusTest
{
    /// <summary>
    /// Verifies when custom latency buckets are set, they are respected
    /// </summary>
    [TestFixture]
    public class CustomBucketsInterceptorTest
    {
        private TestServer _server;
        private TestClient _client;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _server = new TestServer(new ServerMetrics(new []{ 0.199, 0.999 }));
            _client = new TestClient(TestServer.GrpcHostname, TestServer.GrpcPort, 9001, new ClientMetrics(new []{ 0.199, 0.999 }));

            _client.UnaryCall();
            await _client.UnaryCallAsync();
            await _client.ClientStreamingCall();
            await _client.ServerStreamingCall();
            await _client.DuplexStreamingCall();

            _client.UnaryCall(Status.Bad);
            await _client.UnaryCallAsync(Status.Bad);
            await _client.ClientStreamingCall(Status.Bad);
            await _client.ServerStreamingCall(Status.Bad);
            await _client.DuplexStreamingCall(Status.Bad);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _server?.Shutdown();
        }

        [Test]
        public void Client_Latency()
        {
            // We set two custom buckets + the Inf+ bucket
            const int expectedBucketCount = 3;
            List<Line> lines = TestHelper.GetLines(TestClient.MetricsHostname, _client.MetricsPort);

            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestClient.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                l.Type == TestHelper.UnaryTypeName && l.MethodName == _client.UnaryName));
            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestClient.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                  l.Type == TestHelper.ClientStreamingTypeName &&
                                                                  l.MethodName == _client.ClientStreamingName));
            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestClient.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                  l.Type == TestHelper.ServerStreamingTypeName &&
                                                                  l.MethodName == _client.ServerStreamingName));
            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestClient.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                  l.Type == TestHelper.DuplexStreamingTypeName &&
                                                                  l.MethodName == _client.DuplexStreamingName));
        }

        [Test]
        public void Server_Latency()
        {
            // We set two custom buckets + the Inf+ bucket
            const int expectedBucketCount = 3;
            List<Line> lines = TestHelper.GetLines(TestServer.MetricsHostname, TestServer.MetricsPort);

            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestServer.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                  l.Type == TestHelper.UnaryTypeName && l.MethodName == _client.UnaryName));
            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestServer.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                  l.Type == TestHelper.ClientStreamingTypeName &&
                                                                  l.MethodName == _client.ClientStreamingName));
            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestServer.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                  l.Type == TestHelper.ServerStreamingTypeName &&
                                                                  l.MethodName == _client.ServerStreamingName));
            Assert.AreEqual(expectedBucketCount, lines.Count(l => l.MetricsName == TestServer.Metrics.LatencyHistogram.Name + "_bucket" &&
                                                                  l.Type == TestHelper.DuplexStreamingTypeName &&
                                                                  l.MethodName == _client.DuplexStreamingName));
        }
    }
}
