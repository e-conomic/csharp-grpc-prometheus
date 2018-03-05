using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using Prometheus;

namespace NetGrpcPrometheus
{
    public class ServerInterceptor : Interceptor
    {
        private readonly MetricsBase _metrics;
        private readonly string _serviceName;

        public ServerInterceptor(string hostname, int port, string serviceName)
        {
            MetricServer metricServer = new MetricServer(hostname, port);
            metricServer.Start();

            _serviceName = serviceName;
            _metrics = new ServerMetrics();
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string callType = GrpcCallType.Unary;
            string methodName = continuation.Method.Name;

            _metrics.RequestCounterInc(callType, _serviceName, methodName);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task<TResponse> result = continuation(request, context);
            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(callType, _serviceName, methodName,
                    watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(callType, _serviceName, methodName,
                    context.Status.StatusCode);
            }, null, CancellationToken.None);

            return result;
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            string callType = GrpcCallType.ServerStreaming;
            string methodName = continuation.Method.Name;

            _metrics.RequestCounterInc(callType, _serviceName, methodName);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task result = continuation(request, new WrapperServerStreamWriter<TResponse>(responseStream, () =>
                {
                    _metrics.StreamSentCounterInc(callType, _serviceName,
                        methodName);
                }),
                context);

            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(callType, _serviceName, methodName,
                    watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(callType, _serviceName, methodName,
                    context.Status.StatusCode);
            }, null, CancellationToken.None);

            return result;
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            string callType = GrpcCallType.ClientStreaming;
            string methodName = continuation.Method.Name;

            _metrics.RequestCounterInc(callType, _serviceName, methodName);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task<TResponse> result =
                continuation(
                    new WrapperServerStreamReader<TRequest>(requestStream,
                        () =>
                        {
                            _metrics.StreamReceivedCounterInc(callType, _serviceName,
                                methodName);
                        }), context);

            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(callType, _serviceName, methodName,
                    watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(callType, _serviceName, methodName,
                    context.Status.StatusCode);
            }, null, context.CancellationToken);

            return result;

        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            string callType = GrpcCallType.DuplexStreaming;
            string methodName = continuation.Method.Name;

            _metrics.RequestCounterInc(callType, _serviceName, methodName);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task result = continuation(
                new WrapperServerStreamReader<TRequest>(requestStream,
                    () =>
                    {
                        _metrics.StreamReceivedCounterInc(callType, _serviceName,
                            methodName);
                    }),
                new WrapperServerStreamWriter<TResponse>(responseStream,
                    () =>
                    {
                        _metrics.StreamSentCounterInc(callType, _serviceName,
                            methodName);
                    }), context);
            result.ContinueWith((task, o) =>
            {
                watch.Stop();

                _metrics.RecordLatency(callType, _serviceName, methodName,
                    watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(callType, _serviceName, methodName,
                    context.Status.StatusCode);
            }, null, CancellationToken.None);

            return result;
        }
    }
}
