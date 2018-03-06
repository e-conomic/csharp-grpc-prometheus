using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Grpc.Core;
using NetGrpcPrometheus.Models;
using Prometheus;

namespace NetGrpcPrometheus.Helpers
{
    public abstract class MetricsBase
    {
        public abstract bool EnableLatencyMetrics { get; set; }
        public abstract Counter RequestCounter { get; }
        public abstract Counter ResponseCounter { get; }
        public abstract Counter StreamReceivedCounter { get; }
        public abstract Counter StreamSentCounter { get; }
        public abstract Histogram LatencyHistogram { get; }

        public virtual void RequestCounterInc(GrpcMethodInfo method, double inc = 1d)
        {
            RequestCounter.Labels(method.MethodType, method.ServiceName, method.Name).Inc(inc);
        }

        public virtual void ResponseCounterInc(GrpcMethodInfo method, StatusCode code, double inc = 1d)
        {
            ResponseCounter.Labels(method.MethodType, method.ServiceName, method.Name, code.ToString()).Inc(inc);
        }

        public virtual void StreamReceivedCounterInc(GrpcMethodInfo method, double inc = 1d)
        {
            StreamReceivedCounter.Labels(method.MethodType, method.ServiceName, method.Name).Inc(inc);
        }

        public virtual void StreamSentCounterInc(GrpcMethodInfo method, double inc = 1d)
        {
            StreamSentCounter.Labels(method.MethodType, method.ServiceName, method.Name).Inc(inc);
        }

        public virtual void RecordLatency(GrpcMethodInfo method, double value)
        {
            if (EnableLatencyMetrics)
            {
                LatencyHistogram.Labels(method.MethodType, method.ServiceName, method.Name).Observe(value);
            }
        }
    }
}
