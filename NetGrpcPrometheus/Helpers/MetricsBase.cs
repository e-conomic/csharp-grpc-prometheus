using Grpc.Core;
using NetGrpcPrometheus.Models;
using Prometheus;

namespace NetGrpcPrometheus.Helpers
{
    /// <summary>
    /// Base abstract class for metrics
    /// </summary>
    public abstract class MetricsBase
    {
        /// <summary>
        /// Enable recording of latency for responses. By default it's set to false
        /// </summary>
        public abstract bool EnableLatencyMetrics { get; set; }

        /// <summary>
        /// Counter for requests sent from the client side or requests received on the server side
        /// </summary>
        public abstract Counter RequestCounter { get; }

        /// <summary>
        /// Counter for responses received on the client side or responses sent from the server side
        /// </summary>
        public abstract Counter ResponseCounter { get; }

        /// <summary>
        /// Counter for counting total number of messages received from stream - used on both client and server side
        /// </summary>
        public abstract Counter StreamReceivedCounter { get; }

        /// <summary>
        /// Counter for counting total number of messages sent through stream - used on both client and server side
        /// </summary>
        public abstract Counter StreamSentCounter { get; }

        /// <summary>
        /// Histogram for recording latency on both server and client side.
        /// By default it is disabled and can be enabled with <see cref="EnableLatencyMetrics"/>
        /// </summary>
        public abstract Histogram LatencyHistogram { get; }

        /// <summary>
        /// Increments <see cref="RequestCounter"/>
        /// </summary>
        /// <param name="method">Information about the call</param>
        /// <param name="inc">Indicates by how much counter should be incremented. By default it's set to 1</param>
        public virtual void RequestCounterInc(GrpcMethodInfo method, double inc = 1d)
        {
            RequestCounter.Labels(method.MethodType, method.ServiceName, method.Name).Inc(inc);
        }

        /// <summary>
        /// Increments <see cref="ResponseCounter"/>
        /// </summary>
        /// <param name="method">Information about the call</param>
        /// <param name="code">Response status code</param>
        /// <param name="inc">Indicates by how much counter should be incremented. By default it's set to 1</param>
        public virtual void ResponseCounterInc(GrpcMethodInfo method, StatusCode code, double inc = 1d)
        {
            ResponseCounter.Labels(method.MethodType, method.ServiceName, method.Name, code.ToString()).Inc(inc);
        }

        /// <summary>
        /// Increments <see cref="StreamReceivedCounter"/>
        /// </summary>
        /// <param name="method">Information about the call</param>
        /// <param name="inc">Indicates by how much counter should be incremented. By default it's set to 1</param>
        public virtual void StreamReceivedCounterInc(GrpcMethodInfo method, double inc = 1d)
        {
            StreamReceivedCounter.Labels(method.MethodType, method.ServiceName, method.Name).Inc(inc);
        }

        /// <summary>
        /// Increments <see cref="StreamSentCounter"/>
        /// </summary>
        /// <param name="method">Information about the call</param>
        /// <param name="inc">Indicates by how much counter should be incremented. By default it's set to 1</param>
        public virtual void StreamSentCounterInc(GrpcMethodInfo method, double inc = 1d)
        {
            StreamSentCounter.Labels(method.MethodType, method.ServiceName, method.Name).Inc(inc);
        }

        /// <summary>
        /// Records latency recorded during the call
        /// </summary>
        /// <param name="method">Infromation about the call</param>
        /// <param name="value">Value that should be recorded</param>
        public virtual void RecordLatency(GrpcMethodInfo method, double value)
        {
            if (EnableLatencyMetrics)
            {
                LatencyHistogram.Labels(method.MethodType, method.ServiceName, method.Name).Observe(value);
            }
        }
    }
}
