using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using NetGrpcPrometheusTest.Grpc;

namespace NetGrpcPrometheusTest.Helpers
{
    public class TestServer
    {
        public static readonly string GrpcHostname = "127.0.0.1";
        public static readonly int GrpcPort = 50051;
        public static readonly string MetricsHostname = "127.0.0.1";
        public static readonly int MetricsPort = 8080;
        public static readonly string ServiceName = "TestService";

        public static readonly MetricsBase Metrics = new ServerMetrics();

        private readonly Server _server;

        public TestServer()
        {
            _server = new Server()
            {
                Services =
                {
                    TestService.BindService(new TestServiceImp())
                        .Intercept(new ServerInterceptor(MetricsHostname, MetricsPort, ServiceName))
                },
                Ports = {new ServerPort(GrpcHostname, GrpcPort, ServerCredentials.Insecure)}
            };

            _server.Start();
        }

        public void Shutdown()
        {
            _server.ShutdownAsync().Wait();
        }
    }
}
