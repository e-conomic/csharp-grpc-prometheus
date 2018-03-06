using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using Prometheus;

namespace NetGrpcPrometheus
{
    public class ClientInterceptor : Interceptor
    {
        public bool EnableLatencyMetrics
        {
            get => _metrics.EnableLatencyMetrics;
            set => _metrics.EnableLatencyMetrics = value;
        }

        private readonly StatusCode[] _statusCodes;
        private readonly MetricsBase _metrics;

        public ClientInterceptor(string hostname, int port)
        {
            MetricPusher metricPusher = new MetricPusher($"{hostname}:{port}", "job");
            metricPusher.Start();
            
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
            _metrics.ResponseCounterInc(method, StatusCode.OK);

            AsyncServerStreamingCall<TResponse> result = new AsyncServerStreamingCall<TResponse>(
                new WrapperStreamReader<TResponse>(streamingCall.ResponseStream,
                    () => { _metrics.StreamReceivedCounterInc(method); }),
                streamingCall.ResponseHeadersAsync, streamingCall.GetStatus, streamingCall.GetTrailers,
                streamingCall.Dispose);
            
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
            _metrics.ResponseCounterInc(method, StatusCode.OK);
            _metrics.RecordLatency(method, watch.Elapsed.TotalSeconds);

            AsyncDuplexStreamingCall<TRequest, TResponse> result = new AsyncDuplexStreamingCall<TRequest, TResponse>(
                new WrapperClientStreamWriter<TRequest>(streamingCall.RequestStream,
                    () => { _metrics.StreamSentCounterInc(method); }),
                new WrapperStreamReader<TResponse>(streamingCall.ResponseStream,
                    () => { _metrics.StreamReceivedCounterInc(method); }),
                streamingCall.ResponseHeadersAsync, streamingCall.GetStatus, streamingCall.GetTrailers,
                streamingCall.Dispose);

            return result;
        }

    }
}
