using System;
using NetGrpcPrometheus.Helpers;
using Prometheus;

namespace NetGrpcPrometheus.Models
{
    public sealed class ServerMetrics : MetricsBase
    {
        public override bool EnableLatencyMetrics { get; set; }
        public override Counter RequestCounter { get; }
        public override Counter ResponseCounter { get; }
        public override Counter StreamReceivedCounter { get; }
        public override Counter StreamSentCounter { get; }
        public override Histogram LatencyHistogram { get; }

        public ServerMetrics()
        {
            EnableLatencyMetrics = false;

            RequestCounter = Metrics.CreateCounter("grpc_server_started_total",
                "Total number of RPCs started on the server", "grpc_type", "grpc_service", "grpc_method");

            ResponseCounter = Metrics.CreateCounter("grpc_server_handled_total",
                "Total number of RPCs completed on the server, regardless of success or failure", "grpc_type", "grpc_service", "grpc_method", "grpc_code");

            StreamReceivedCounter = Metrics.CreateCounter("grpc_server_msg_received_total",
                "Total number of RPC stream messages received on the server", "grpc_type", "grpc_service",
                "grpc_method");

            StreamSentCounter = Metrics.CreateCounter("grpc_server_msg_sent_total",
                "Total number of gRPC stream messages sent by the server", "grpc_type", "grpc_service", "grpc_method");

            LatencyHistogram = Metrics.CreateHistogram("grpc_server_handling_seconds",
                "Histogram of response latency (seconds) of gRPC",
                new[] { .001, .005, .01, .05, 0.075, .1, .25, .5, 1, 2, 5, 10 }, "grpc_type", "grpc_service",
                "grpc_method");
        }
    }
}
