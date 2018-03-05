using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Grpc.Core;
using Prometheus;

namespace NetGrpcPrometheus.Helpers
{
    public abstract class MetricsBase
    {
        public abstract Counter RequestCounter { get; }
        public abstract Counter ResponseCounter { get; }
        public abstract Counter StreamReceivedCounter { get; }
        public abstract Counter StreamSentCounter { get; }
        public abstract Histogram LatencyHistogram { get; }
        
        public virtual void RequestCounterInc(string callType, string serviceName, string methodName, double inc = 1d)
        {
            RequestCounter.Labels(callType, serviceName, methodName).Inc(inc);
        }

        public virtual void ResponseCounterInc(string callType, string serviceName, string method, StatusCode code, double inc = 1d)
        {
            ResponseCounter.Labels(callType, serviceName, method, code.ToString()).Inc(inc);
        }

        public virtual void StreamReceivedCounterInc(string callType, string serviceName, string methodName,
            double inc = 1d)
        {
            StreamReceivedCounter.Labels(callType, serviceName, methodName).Inc(inc);
        }

        public virtual void StreamSentCounterInc(string callType, string serviceName, string methodName,
            double inc = 1d)
        {
            StreamSentCounter.Labels(callType, serviceName, methodName).Inc(inc);
        }

        public virtual void RecordLatency(string callType, string serviceName, string method, double value)
        {
            LatencyHistogram.Labels(callType, serviceName, method).Observe(value);
        }
    }

    public static class GrpcCallType
    {
        public const string Unary = "unary";
        public const string ServerStreaming = "server_stream";
        public const string ClientStreaming = "client_stream";
        public const string DuplexStreaming = "bidi_stream";
    }
}
