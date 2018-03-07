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

            int fakeGrpcPort = 50050;

            channel = new Channel(TestServer.GrpcHostname, fakeGrpcPort, ChannelCredentials.Insecure);
            _client = new TestService.TestServiceClient(
                channel.Intercept(interceptor));

            UnaryCall();
            UnaryCallAsync().Wait();
            ClientStreamingCall().Wait();
            ServerStreamingCall().Wait();
            DuplexStreamingCall().Wait();

            Task.Run(() => { Thread.Sleep(2000); }).Wait();
        }

        private void UnaryCall()
        {
            try
            {
                _client.UnaryPing(new PingRequest() { Value = 1 });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task UnaryCallAsync()
        {
            try
            {
                await _client.UnaryPingAsync(new PingRequest() { Value = 1 });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async Task ClientStreamingCall()
        {
            try
            {
                using (AsyncClientStreamingCall<PingRequest, PingResponse> call = _client.ClientStreamingPing())
                {
                    await call.RequestStream.WriteAsync(new PingRequest() { Value = 1 });
                    await call.RequestStream.CompleteAsync();
                    await call.ResponseAsync;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            
        }

        private async Task ServerStreamingCall()
        {
            try
            {
                using (AsyncServerStreamingCall<PingResponse> call =
                    _client.ServerStreamingPing(new PingRequest() { Value = 1 }))
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

        private async Task DuplexStreamingCall()
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

                    await call.RequestStream.WriteAsync(new PingRequest() { Value = 1 });
                    await call.RequestStream.WriteAsync(new PingRequest() { Value = 1 });
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
