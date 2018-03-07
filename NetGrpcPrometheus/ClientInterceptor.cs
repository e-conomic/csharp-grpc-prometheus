using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using Prometheus;
using Prometheus.Advanced;

namespace NetGrpcPrometheus
{
    /// <summary>
    /// Interceptor for intercepting calls on client side
    /// </summary>
    public class ClientInterceptor : Interceptor
    {
        /// <summary>
        /// Enable recording of latency for responses. By default it's set to false
        /// </summary>
        public bool EnableLatencyMetrics
        {
            get => _metrics.EnableLatencyMetrics;
            set => _metrics.EnableLatencyMetrics = value;
        }

        private readonly StatusCode[] _statusCodes;
        private readonly MetricsBase _metrics;

        /// <summary>
        /// Constructor for client side interceptor with metric server.
        /// Metric server will be created and provide metrics on /metrics endpoint.
        /// </summary>
        /// <param name="hostname">Host name for Prometheus metrics server - e.g. localhost</param>
        /// <param name="port">Port for Prometheus server</param>
        /// <param name="defaultMetrics">Indicates if Prometheus metrics server should record default metrics</param>
        public ClientInterceptor(string hostname, int port, bool defaultMetrics = true)
        {
            MetricServer metricServer = new MetricServer(hostname, port);
            metricServer.Start();

            if (!defaultMetrics)
            {
                DefaultCollectorRegistry.Instance.Clear();
            }

            _metrics = new ClientMetrics();
            _statusCodes = Enum.GetValues(typeof(StatusCode)).Cast<StatusCode>().ToArray();
        }

        /// <summary>
        /// Constructor for client side interceptor with metric pusher.
        /// Metric pusher will be created and will push metrics to the endpoint specified pushgateway
        /// </summary>
        /// <param name="endpoint">Endpoint for pushgateway - e.g. http://pushgateway.example.org:9091/metrics</param>
        /// <param name="job"></param>
        /// <param name="defaultMetrics"></param>
        /// <param name="instance"></param>
        /// <param name="intervalMilliseconds"></param>
        /// <param name="additionalLabels"></param>
        /// <param name="registry"></param>
        public ClientInterceptor(string endpoint, string job, bool defaultMetrics = true, string instance = null, ulong intervalMilliseconds = 1000, IEnumerable<Tuple<string,string>> additionalLabels = null, ICollectorRegistry registry = null)
        {
            MetricPusher metricServer = new MetricPusher(endpoint, job, instance, (long) intervalMilliseconds, additionalLabels, registry);
            metricServer.Start();

            if (!defaultMetrics)
            {
                DefaultCollectorRegistry.Instance.Clear();
            }

            _metrics = new ClientMetrics();
            _statusCodes = Enum.GetValues(typeof(StatusCode)).Cast<StatusCode>().ToArray();
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            TResponse result;
            StatusCode code = StatusCode.OK;

            try
            {
                result = continuation(request, context);
            }
            catch (Exception e)
            {
                code = _statusCodes.FirstOrDefault(s => e.Message.Contains(s.ToString()));

                throw;
            }
            finally
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
                _metrics.ResponseCounterInc(method, code);
            }
 
            return result;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            AsyncUnaryCall<TResponse> result = continuation(request, context);

            result.ResponseAsync.ContinueWith(task =>
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);

                StatusCode code = task.Exception == null
                    ? StatusCode.OK
                    : _statusCodes.FirstOrDefault(s => task.Exception.Message.Contains(s.ToString()));
                
                _metrics.ResponseCounterInc(method, code);
            });

            return result;
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            AsyncClientStreamingCall<TRequest, TResponse> streamingCall = continuation(context);
            AsyncClientStreamingCall<TRequest, TResponse> result = new AsyncClientStreamingCall<TRequest, TResponse>(
                new WrapperClientStreamWriter<TRequest>(streamingCall.RequestStream,
                    () => { _metrics.StreamSentCounterInc(method); }),
                streamingCall.ResponseAsync,
                streamingCall.ResponseHeadersAsync, streamingCall.GetStatus, streamingCall.GetTrailers,
                streamingCall.Dispose);

            result.ResponseAsync.ContinueWith(task =>
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);

                StatusCode code = task.Exception == null
                    ? StatusCode.OK
                    : _statusCodes.FirstOrDefault(s => task.Exception.Message.Contains(s.ToString()));

                _metrics.ResponseCounterInc(method, code);
            });

            return result;
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            AsyncServerStreamingCall<TResponse> streamingCall = continuation(request, context);

            watch.Stop();
            _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
            // TODO: Investigate whether or not any response code should be returned on server streaming
            _metrics.ResponseCounterInc(method, StatusCode.OK);

            AsyncServerStreamingCall<TResponse> result = new AsyncServerStreamingCall<TResponse>(
                new WrapperStreamReader<TResponse>(streamingCall.ResponseStream,
                    () => { _metrics.StreamReceivedCounterInc(method); }),
                streamingCall.ResponseHeadersAsync, streamingCall.GetStatus, streamingCall.GetTrailers,
                streamingCall.Dispose);

            return result;
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            AsyncDuplexStreamingCall<TRequest, TResponse> streamingCall = continuation(context);

            watch.Stop();
            _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);
            // TODO: Investigate whether or not any response code should be returned on server streaming
            _metrics.ResponseCounterInc(method, StatusCode.OK);

            WrapperStreamReader<TResponse> responseStream =
                new WrapperStreamReader<TResponse>(streamingCall.ResponseStream,
                    () => { _metrics.StreamReceivedCounterInc(method); });
            AsyncDuplexStreamingCall<TRequest, TResponse> result = new AsyncDuplexStreamingCall<TRequest, TResponse>(
                new WrapperClientStreamWriter<TRequest>(streamingCall.RequestStream,
                    () => { _metrics.StreamSentCounterInc(method); }), responseStream,
                streamingCall.ResponseHeadersAsync, streamingCall.GetStatus, streamingCall.GetTrailers,
                streamingCall.Dispose);

            return result;
        }

    }
}
