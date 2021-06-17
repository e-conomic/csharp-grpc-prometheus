using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using Prometheus;

namespace NetGrpcPrometheus
{
    /// <summary>
    /// Interceptor for intercepting calls on client side
    /// </summary>
    public class ClientInterceptor : Interceptor
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
        /// Constructor for client side interceptor with metric server.
        /// Metric server will be created and provide metrics on /metrics endpoint.
        /// </summary>
        /// <param name="enableLatencyMetrics">Enable recording of latency for responses. By default it's set to false</param>
        public ClientInterceptor(bool enableLatencyMetrics = false)
        {
            _metrics = new ClientMetrics();
            EnableLatencyMetrics = enableLatencyMetrics;
            //_statusCodes = Enum.GetValues(typeof(StatusCode)).Cast<StatusCode>().ToArray();
        }

        /// <summary>
        /// Constructor for client side interceptor with metric pusher.
        /// Metric pusher will be created and will push metrics to the endpoint specified pushgateway
        /// </summary>
        /// <param name="endpoint">Endpoint for pushgateway - e.g. http://pushgateway.example.org:9091/metrics</param>
        /// <param name="job"></param>
        /// <param name="defaultMetrics"></param>
        /// <param name="enableLatencyMetrics">Enable recording of latency for responses. By default it's set to false</param>
        /// <param name="instance"></param>
        /// <param name="intervalMilliseconds"></param>
        /// <param name="additionalLabels"></param>
        /// <param name="registry"></param>
        public ClientInterceptor(string endpoint, string job, bool defaultMetrics = true, bool enableLatencyMetrics = false,
            string instance = null, ulong intervalMilliseconds = 1000,
            IEnumerable<Tuple<string, string>> additionalLabels = null)
        {
            var metricServer = new MetricPusher(endpoint, job, instance, (long) intervalMilliseconds,
                additionalLabels);
            metricServer.Start();

            _metrics = new ClientMetrics();
            EnableLatencyMetrics = enableLatencyMetrics;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            var watch = Stopwatch.StartNew();

            TResponse result;

            try
            {
                result = continuation(request, context);
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

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            var watch = Stopwatch.StartNew();

            AsyncUnaryCall<TResponse> result = continuation(request, context);
            
            result.ResponseAsync.ContinueWith(task =>
            {
                watch.Stop();
                _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);

                if (task.Exception == null)
                {
                    _metrics.ResponseCounterInc(method, StatusCode.OK);
                }
                else
                {
                    RpcException exception = (RpcException) task.Exception.InnerException;
                    _metrics.ResponseCounterInc(method, exception.Status.StatusCode);
                }
            });

            return result;
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            var watch = Stopwatch.StartNew();

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

                if (task.Exception == null)
                {
                    _metrics.ResponseCounterInc(method, StatusCode.OK);
                }
                else
                {
                    RpcException exception = (RpcException)task.Exception.InnerException;
                    _metrics.ResponseCounterInc(method, exception.Status.StatusCode);
                }
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

            var watch = Stopwatch.StartNew();

            AsyncServerStreamingCall<TResponse> result;

            try
            {
                AsyncServerStreamingCall<TResponse> streamingCall = continuation(request, context);

                result =
                    new AsyncServerStreamingCall<TResponse>(
                        new WrapperStreamReader<TResponse>(
                            streamingCall.ResponseStream,
                            () => { _metrics.StreamReceivedCounterInc(method); },
                            statusCode => { _metrics.ResponseCounterInc(method, statusCode); }),
                        streamingCall.ResponseHeadersAsync,
                        streamingCall.GetStatus,
                        streamingCall.GetTrailers,
                        streamingCall.Dispose);

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

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            GrpcMethodInfo method = new GrpcMethodInfo(context.Method.FullName, context.Method.Type);

            _metrics.RequestCounterInc(method);

            var watch = Stopwatch.StartNew();

            AsyncDuplexStreamingCall<TRequest, TResponse> result;

            try
            {
                AsyncDuplexStreamingCall<TRequest, TResponse> streamingCall = continuation(context);

                WrapperStreamReader<TResponse> responseStream =
                    new WrapperStreamReader<TResponse>(
                        streamingCall.ResponseStream,
                        () => { _metrics.StreamReceivedCounterInc(method); },
                        statusCode => { _metrics.ResponseCounterInc(method, statusCode); });

                result = new AsyncDuplexStreamingCall<TRequest, TResponse>(
                    new WrapperClientStreamWriter<TRequest>(streamingCall.RequestStream,
                        () => { _metrics.StreamSentCounterInc(method); }), responseStream,
                    streamingCall.ResponseHeadersAsync, streamingCall.GetStatus, streamingCall.GetTrailers,
                    streamingCall.Dispose);

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
