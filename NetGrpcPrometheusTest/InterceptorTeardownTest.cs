using System.Threading.Tasks;
using NetGrpcPrometheusTest.Grpc;
using NetGrpcPrometheusTest.Helpers;
using NUnit.Framework;

namespace NetGrpcPrometheusTest
{  
    /// <summary>
    /// When Interceptor disposed should release all ports.
    /// </summary>
    [TestFixture]
    public class InterceptorTeardownTest
    {
        private TestServer _server;
        private TestClient _client;

        [SetUp]
        public async Task SetUp()
        {
            _server = new TestServer();
            _client = new TestClient(TestServer.GrpcHostname, TestServer.GrpcPort, 9001);

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
        [Test]
        public void TestFirstInvocationTeardown() {}
    
        [Test]
        public void TestSecondInvocationTeardown() {}

        [TearDown]
        public void TearDown()
        {
            // These disposes should free the metric port. If not, the second SetUp call will fail.
            _server.Dispose();
            _client.Dispose();
        }
    }
}