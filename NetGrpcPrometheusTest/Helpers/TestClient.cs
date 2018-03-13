using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NetGrpcPrometheus;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;

namespace NetGrpcPrometheusTest.Helpers
{
    public class TestClient
    {
        public static readonly MetricsBase Metrics = new ClientMetrics();
        public static readonly string MetricsHostname = "127.0.0.1";
        public static readonly int MetricsPort = 50053;

        public string UnaryName => nameof(_client.UnaryPing);
        public string ClientStreamingName => nameof(_client.ClientStreamingPing);
        public string ServerStreamingName => nameof(_client.ServerStreamingPing);
        public string DuplexStreamingName => nameof(_client.DuplexPing);

        private readonly TestService.TestServiceClient _client;

        public TestClient()
        {
            ClientInterceptor interceptor =
                new ClientInterceptor(MetricsHostname, MetricsPort) {EnableLatencyMetrics = true};

            Channel channel = new Channel(TestServer.GrpcHostname, TestServer.GrpcPort, ChannelCredentials.Insecure);
            _client = new TestService.TestServiceClient(
                channel.Intercept(interceptor));

            UnaryCall();
            UnaryCallAsync().Wait();
            ClientStreamingCall().Wait();
            ServerStreamingCall().Wait();
            DuplexStreamingCall().Wait();

            UnaryCall(Status.Bad);
            UnaryCallAsync(Status.Bad).Wait();
            ClientStreamingCall(Status.Bad).Wait();
            ServerStreamingCall(Status.Bad).Wait();
            DuplexStreamingCall(Status.Bad).Wait();

            Task.Run(() => { Thread.Sleep(2000); }).Wait();
        }

        private void UnaryCall(Status status = Status.Ok)
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

        private async Task UnaryCallAsync(Status status = Status.Ok)
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

        private async Task ClientStreamingCall(Status status = Status.Ok)
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

        private async Task ServerStreamingCall(Status status = Status.Ok)
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

        private async Task DuplexStreamingCall(Status status = Status.Ok)
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
    }
}
