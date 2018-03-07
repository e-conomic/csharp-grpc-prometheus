using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using Prometheus;
using Prometheus.Advanced;

namespace NetGrpcPrometheus
{
    /// <summary>
    /// Interceptor for intercepting calls on server side 
    /// </summary>
    public class ServerInterceptor : Interceptor
    {
        /// <summary>
        /// Enable recording of latency for responses. By default it's set to false
        /// </summary>
        public bool EnableLatencyMetrics
        {
            get => _metrics.EnableLatencyMetrics;
            set => _metrics.EnableLatencyMetrics = value;
        }

        private readonly MetricsBase _metrics;

        /// <summary>
        /// Constructor for server side interceptor
        /// </summary>
        /// <param name="hostname">Host name for Prometheus metrics server - e.g. localhost</param>
        /// <param name="port">Port for Prometheus server</param>
        /// <param name="defaultMetrics">Indicates if Prometheus metrics server should record default metrics</param>
        public ServerInterceptor(string hostname, int port, bool defaultMetrics = true)
        {
            MetricServer metricServer = new MetricServer(hostname, port);
            metricServer.Start();

            if (!defaultMetrics)
            {
                DefaultCollectorRegistry.Instance.Clear();
            }

            _metrics = new ServerMetrics();
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method, MethodType.Unary);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task<TResponse> result = continuation(request, context);
            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(method, context.Status.StatusCode);
            }, null, CancellationToken.None);

            return result;
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method, MethodType.ServerStreaming);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task result = continuation(request,
                new WrapperServerStreamWriter<TResponse>(responseStream,
                    () => { _metrics.StreamSentCounterInc(method); }),
                context);

            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(method, context.Status.StatusCode);
            }, null, CancellationToken.None);

            return result;
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method, MethodType.ClientStreaming);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task<TResponse> result =
                continuation(
                    new WrapperStreamReader<TRequest>(requestStream,
                        () => { _metrics.StreamReceivedCounterInc(method); }), context);

            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(method, context.Status.StatusCode);
            }, null, context.CancellationToken);

            return result;

        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method, MethodType.DuplexStreaming);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task result = continuation(
                new WrapperStreamReader<TRequest>(requestStream,
                    () => { _metrics.StreamReceivedCounterInc(method); }),
                new WrapperServerStreamWriter<TResponse>(responseStream,
                    () => { _metrics.StreamSentCounterInc(method); }), context);
            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(method, context.Status.StatusCode);
            }, null, CancellationToken.None);

            return result;
        }
    }
}
