using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus;
using NetGrpcPrometheusTest.Grpc;
using Prometheus;

namespace NetGrpcPrometheusTest.Helpers
{
    public class TestAsyncServer
    {
        public static readonly string GrpcHostname = "127.0.0.1";
        public static readonly int GrpcPort = 50052;
        public static readonly string MetricsHostname = "127.0.0.1";
        public static readonly int MetricsPort = 9004;

        private readonly MetricServer _metricsServer;
        private readonly Server _server;

        public TestAsyncServer()
        {
            _metricsServer = new MetricServer(MetricsHostname, MetricsPort);
            _metricsServer.Start();
            var interceptor = new ServerInterceptor { EnableLatencyMetrics = true };

            _server = new Server()
            {
                Services =
                {
                    TestService.BindService(new TestServiceAsyncImp()).Intercept(interceptor)
                },
                Ports = { new ServerPort(GrpcHostname, GrpcPort, ServerCredentials.Insecure) }
            };

            _server.Start();
        }

        public void Shutdown()
        {
            _metricsServer.StopAsync().Wait();
            _server.ShutdownAsync().Wait();
        }
    }
}
