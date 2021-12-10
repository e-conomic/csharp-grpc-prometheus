using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NetGrpcPrometheus
{
    /// <summary>
    /// Interceptor for intercepting calls on server side 
    /// </summary>
    public class ServerInterceptor : Interceptor
    {
        private readonly MetricsBase _metrics;

        /// <summary>
        /// Enable recording of latency for responses. By default it's set to false
        /// </summary>
        public bool EnableLatencyMetrics
        {
            get => _metrics.EnableLatencyMetrics;
            set => _metrics.EnableLatencyMetrics = value;
        }

        /// <summary>
        /// Constructor for server side interceptor
        /// </summary>
        /// <param name="enableLatencyMetrics">Enable recording of latency for responses. By default it's set to false</param>
        /// <param name="metrics">The metrics object to use, allowing customization of metrics produced. By default, will create a new instance with no customization.</param>
        public ServerInterceptor(bool enableLatencyMetrics = false, ServerMetrics metrics = null)
        {
            _metrics = metrics ?? new ServerMetrics();
            EnableLatencyMetrics = enableLatencyMetrics;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var method = new GrpcMethodInfo(context.Method, MethodType.Unary);

            _metrics.RequestCounterInc(method);

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var result = await continuation(request, context);
                _metrics.ResponseCounterInc(method, context.Status.StatusCode);
                return result;
            }
            catch (RpcException e)
            {
                _metrics.ResponseCounterInc(method, e.Status.StatusCode);
                throw;
            }
            finally
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
            }
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method, MethodType.ServerStreaming);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task result;

            try
            {
                result = continuation(request,
                    new WrapperServerStreamWriter<TResponse>(responseStream,
                        () => { _metrics.StreamSentCounterInc(method); }),
                    context);

                _metrics.ResponseCounterInc(method, StatusCode.OK);
            }
            catch (RpcException e)
            {
                _metrics.ResponseCounterInc(method, e.Status.StatusCode);
                throw;
            }
            finally
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
            }

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

            Task<TResponse> result;

            try
            {
                result = continuation(
                    new WrapperStreamReader<TRequest>(
                        requestStream,
                        () => { _metrics.StreamReceivedCounterInc(method); },
                        statusCode => { _metrics.ResponseCounterInc(method, statusCode); }),
                    context);

                _metrics.ResponseCounterInc(method, StatusCode.OK);
            }
            catch (RpcException e)
            {
                _metrics.ResponseCounterInc(method, e.Status.StatusCode);
                throw;
            }
            finally
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
            }

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

            Task result;

            try
            {
                result = continuation(
                    new WrapperStreamReader<TRequest>(
                        requestStream,
                        () => { _metrics.StreamReceivedCounterInc(method); },
                        statusCode => { _metrics.ResponseCounterInc(method, statusCode); }),
                    new WrapperServerStreamWriter<TResponse>(responseStream,
                        () => { _metrics.StreamSentCounterInc(method); }), context);

                _metrics.ResponseCounterInc(method, StatusCode.OK);
            }
            catch (RpcException e)
            {
                _metrics.ResponseCounterInc(method, e.Status.StatusCode);
                throw;
            }
            finally
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
            }

            return result;
        }
    }
}
