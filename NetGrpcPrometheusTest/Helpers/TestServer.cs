using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using NetGrpcPrometheusTest.Grpc;
using Prometheus;

namespace NetGrpcPrometheusTest.Helpers
{
    public class TestServer : IDisposable
    {
        public static readonly string GrpcHostname = "127.0.0.1";
        public static readonly int GrpcPort = 50051;
        public static readonly string MetricsHostname = "127.0.0.1";
        public static readonly int MetricsPort = 9003;

        public static readonly MetricsBase Metrics = new ServerMetrics();

        private readonly MetricServer _metricsServer;
        private readonly Server _server;
        private readonly ServerInterceptor _interceptor;

        public TestServer()
        {
            _metricsServer = new MetricServer(MetricsHostname, MetricsPort);
            _metricsServer.Start();
            _interceptor =
                new ServerInterceptor {EnableLatencyMetrics = true};
            
            _server = new Server()
            {
                Services =
                {
                    TestService.BindService(new TestServiceImp()).Intercept(_interceptor)
                },
                Ports = {new ServerPort(GrpcHostname, GrpcPort, ServerCredentials.Insecure)}
            };

            _server.Start();
        }

        public void Shutdown()
        {
            _server.ShutdownAsync().Wait();
            _metricsServer.Stop();
        }

        public void Dispose()
        {
            Shutdown();
        }
    }
}
