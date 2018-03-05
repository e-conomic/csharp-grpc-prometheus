using System;
using NetGrpcPrometheus.Helpers;
using Prometheus;

namespace NetGrpcPrometheus.Models
{
    public class ClientMetrics : MetricsBase
    {
        public override Counter RequestCounter { get; }
        public override Counter ResponseCounter { get; }
        public override Counter StreamReceivedCounter { get; }
        public override Counter StreamSentCounter { get; }
        public override Histogram LatencyHistogram { get; }

        public ClientMetrics()
        {
            RequestCounter = Metrics.CreateCounter("grpc_client_started_total",
                "Total number of RPCs started on the client", "grpc_type", "grpc_service", "grpc_method");

            ResponseCounter = Metrics.CreateCounter("grpc_client_handled_total",
                "Total number of RPCs completed by the client, regardless of success or failure", "grpc_type", "grpc_service", "grpc_method", "grpc_code");

            StreamReceivedCounter = Metrics.CreateCounter("grpc_client_msg_received_total",
                "Total number of RPC stream messages received by the client", "grpc_type", "grpc_service",
                "grpc_method");

            StreamSentCounter = Metrics.CreateCounter("grpc_client_msg_sent_total",
                "Total number of gRPC stream messages sent by the client", "grpc_type", "grpc_service", "grpc_method");

            LatencyHistogram = Metrics.CreateHistogram("grpc_client_handling_seconds",
                "Histogram of response latency (seconds) of the gRPC",
                new[] { .001, .005, .01, .05, 0.075, .1, .25, .5, 1, 2, 5, 10 }, "grpc_type", "grpc_service",
                "grpc_method");
        }
    }
}
