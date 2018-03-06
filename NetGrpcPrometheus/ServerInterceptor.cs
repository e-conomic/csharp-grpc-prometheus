using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using Prometheus;

namespace NetGrpcPrometheus
{
    public class ServerInterceptor : Interceptor
    {
        public bool EnableLatencyMetrics
        {
            get => _metrics.EnableLatencyMetrics;
            set => _metrics.EnableLatencyMetrics = value;
        }

        private readonly MetricsBase _metrics;

        public ServerInterceptor(string hostname, int port)
        {
            MetricServer metricServer = new MetricServer(hostname, port);
            metricServer.Start();

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
