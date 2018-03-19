using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NetGrpcPrometheusTest.Helpers;
using NetGrpcPrometheusTest.Utils;
using NUnit.Framework;
using Status = NetGrpcPrometheusTest.Grpc.Status;

namespace NetGrpcPrometheusTest
{
    [TestFixture]
    public class AsyncInterceptorTest
    {
        private TestAsyncServer _server;
        private TestClient _client;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _server = new TestAsyncServer();
            _client = new TestClient(TestAsyncServer.GrpcHostname, TestAsyncServer.GrpcPort, 9002);
            
            await _client.UnaryCallAsync();
            await _client.ClientStreamingCall();
            await _client.ServerStreamingCall();
            await _client.DuplexStreamingCall();
            
            await _client.UnaryCallAsync(Status.Bad);
            await _client.ClientStreamingCall(Status.Bad);
            await _client.ServerStreamingCall(Status.Bad);
            await _client.DuplexStreamingCall(Status.Bad);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _server.Shutdown();
        }

        [Test]
        public void Client_Response()
        {
            List<Line> lines = TestHelper.GetLines(TestClient.MetricsHostname, _client.MetricsPort);

            Assert.IsTrue(lines.Any(l => l.MetricsName == TestClient.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.UnaryTypeName && l.MethodName == _client.UnaryName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
            Assert.IsTrue(lines.Any(l => l.MetricsName == TestClient.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.ClientStreamingTypeName &&
                                         l.MethodName == _client.ClientStreamingName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
            Assert.IsTrue(lines.Any(l => l.MetricsName == TestClient.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.ServerStreamingTypeName &&
                                         l.MethodName == _client.ServerStreamingName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
            Assert.IsTrue(lines.Any(l => l.MetricsName == TestClient.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.DuplexStreamingTypeName &&
                                         l.MethodName == _client.DuplexStreamingName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
        }

        [Test]
        public void Client_Response_Internal()
        {
            List<Line> lines = TestHelper.GetLines(TestClient.MetricsHostname, _client.MetricsPort);

            Assert.IsTrue(lines.Any(l => l.MetricsName == TestClient.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.UnaryTypeName &&
                                         l.MethodName == _client.UnaryName &&
                                         l.StatusCode == StatusCode.Internal.ToString()));
            Assert.IsTrue(lines.Any(l => l.MetricsName == TestClient.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.ClientStreamingTypeName &&
                                         l.MethodName == _client.ClientStreamingName &&
                                         l.StatusCode == StatusCode.Internal.ToString()));
        }

        [Test]
        public void Server_Response()
        {
            List<Line> lines = TestHelper.GetLines(TestAsyncServer.MetricsHostname, TestAsyncServer.MetricsPort);

            Assert.IsTrue(lines.Any(l => l.MetricsName == TestServer.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.UnaryTypeName && l.MethodName == _client.UnaryName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
            Assert.IsTrue(lines.Any(l => l.MetricsName == TestServer.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.ClientStreamingTypeName &&
                                         l.MethodName == _client.ClientStreamingName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
            Assert.IsTrue(lines.Any(l => l.MetricsName == TestServer.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.ServerStreamingTypeName &&
                                         l.MethodName == _client.ServerStreamingName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
            Assert.IsTrue(lines.Any(l => l.MetricsName == TestServer.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.DuplexStreamingTypeName &&
                                         l.MethodName == _client.DuplexStreamingName &&
                                         l.StatusCode == StatusCode.OK.ToString()));
        }

        [Test]
        public void Server_Response_Internal()
        {
            List<Line> lines = TestHelper.GetLines(TestAsyncServer.MetricsHostname, TestAsyncServer.MetricsPort);

            Assert.IsTrue(lines.Any(l => l.MetricsName == TestServer.Metrics.ResponseCounter.Name &&
                                         l.Type == TestHelper.UnaryTypeName && l.MethodName == _client.UnaryName &&
                                         l.StatusCode == StatusCode.Internal.ToString()));
        }
    }
}
