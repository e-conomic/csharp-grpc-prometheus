﻿using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;
using NetGrpcPrometheusTest.Grpc;
using Prometheus;
using System;
using System.Threading;
using System.Threading.Tasks;
using Status = NetGrpcPrometheusTest.Grpc.Status;

namespace NetGrpcPrometheusTest.Helpers
{
    public class TestClient : IDisposable
    {
        public static readonly MetricsBase Metrics = new ClientMetrics();
        public static readonly string MetricsHostname = "127.0.0.1";

        public string UnaryName => nameof(_client.UnaryPing);
        public string ClientStreamingName => nameof(_client.ClientStreamingPing);
        public string ServerStreamingName => nameof(_client.ServerStreamingPing);
        public string DuplexStreamingName => nameof(_client.DuplexPing);
        public int MetricsPort;

        private readonly MetricServer _metricsServer;

        private readonly TestService.TestServiceClient _client;
        private ClientInterceptor _interceptor;

        public TestClient(string grpcHostName, int grpcPort, int metricsPort)
        {
            MetricsPort = metricsPort;
            _metricsServer = new MetricServer(MetricsHostname, MetricsPort);
            _metricsServer.Start();
            _interceptor = new ClientInterceptor(true);

            var channel = new Channel(grpcHostName, grpcPort, ChannelCredentials.Insecure);
            _client = new TestService.TestServiceClient(
                channel.Intercept(_interceptor));
        }

        public void UnaryCall(Status status = Status.Ok)
        {
            try
            {
                _client.UnaryPing(new PingRequest() { Status = status });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task UnaryCallAsync(Status status = Status.Ok)
        {
            try
            {
                await _client.UnaryPingAsync(new PingRequest() { Status = status });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task ClientStreamingCall(Status status = Status.Ok)
        {
            try
            {
                using (AsyncClientStreamingCall<PingRequest, PingResponse> call = _client.ClientStreamingPing())
                {
                    await call.RequestStream.WriteAsync(new PingRequest() { Status = status });
                    await call.RequestStream.CompleteAsync();
                    await call.ResponseAsync;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            
        }

        public async Task ServerStreamingCall(Status status = Status.Ok)
        {
            try
            {
                using (AsyncServerStreamingCall<PingResponse> call =
                    _client.ServerStreamingPing(new PingRequest() { Status = status }))
                {
                    while (await call.ResponseStream.MoveNext(CancellationToken.None))
                    {
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task DuplexStreamingCall(Status status = Status.Ok)
        {
            try
            {
                using (AsyncDuplexStreamingCall<PingRequest, PingResponse> call = _client.DuplexPing())
                {
                    Task responseReaderTask = Task.Run(async () =>
                    {
                        while (call != null && await call.ResponseStream.MoveNext())
                        {
                        }
                    });

                    await call.RequestStream.WriteAsync(new PingRequest() { Status = status });
                    await call.RequestStream.WriteAsync(new PingRequest() { Status = status });
                    await call.RequestStream.CompleteAsync();

                    await responseReaderTask;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Wait()
        {
            Task.Run(() => { Thread.Sleep(2000); }).Wait();
        }

        public void Dispose()
        {
            _metricsServer.StopAsync().Wait();
            _interceptor.Dispose();
        }
    }
}
