using NetGrpcPrometheus.Helpers;
using Prometheus;

namespace NetGrpcPrometheus.Models
{
    /// <summary>
    /// Implementation of <see cref="MetricsBase"/>. 
    /// Creates specific names and labes for metrics from the base class.
    /// </summary>
    public sealed class ClientMetrics : MetricsBase
    {
        public override bool EnableLatencyMetrics { get; set; }
        public override Counter RequestCounter { get; }
        public override Counter ResponseCounter { get; }
        public override Counter StreamReceivedCounter { get; }
        public override Counter StreamSentCounter { get; }
        public override Histogram LatencyHistogram { get; }
        
        public ClientMetrics(double[] latencyHistogramBuckets = null)
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
                new HistogramConfiguration
                {
                    Buckets = latencyHistogramBuckets ?? new[] { .001, .005, .01, .05, 0.075, .1, .25, .5, 1, 2, 5, 10 },
                    LabelNames = new []{"grpc_type", "grpc_service", "grpc_method"}
                }
                );
        }
    }
}
