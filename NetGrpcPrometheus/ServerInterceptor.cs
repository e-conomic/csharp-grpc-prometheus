using System.Diagnostics;
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
        /// <param name="enableLatencyMetrics">Enable recording of latency for responses. By default it's set to false</param>
        public ServerInterceptor(string hostname, int port, bool defaultMetrics = true, bool enableLatencyMetrics = false)
        {
            MetricServer metricServer = new MetricServer(hostname, port);
            metricServer.Start();
            
            if (!defaultMetrics)
            {
                DefaultCollectorRegistry.Instance.Clear();
            }

            _metrics = new ServerMetrics();
            EnableLatencyMetrics = enableLatencyMetrics;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method, MethodType.Unary);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            try
            {
                TResponse result = await continuation(request, context);
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
                    new WrapperStreamReader<TRequest>(requestStream,
                        () => { _metrics.StreamReceivedCounterInc(method); }), context);

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
                    new WrapperStreamReader<TRequest>(requestStream,
                        () => { _metrics.StreamReceivedCounterInc(method); }),
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
