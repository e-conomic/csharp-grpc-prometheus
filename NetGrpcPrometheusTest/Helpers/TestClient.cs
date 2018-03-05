using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NetGrpcPrometheus.Helpers;
using NetGrpcPrometheus.Models;

namespace NetGrpcPrometheusTest.Helpers
{
    public class TestClient
    {
        public static readonly MetricsBase ServerMetrics = new ClientMetrics();

        public string UnaryName => nameof(_client.UnaryPing);
        public string ClientStreamingName => nameof(_client.ClientStreamingPing);
        public string ServerStreamingName => nameof(_client.ServerStreamingPing);
        public string DuplexStreamingName => nameof(_client.DuplexPing);

        private readonly TestService.TestServiceClient _client;

        public TestClient()
        {
            Channel channel = new Channel(TestServer.GrpcHostname, TestServer.GrpcPort, ChannelCredentials.Insecure);
            _client = new TestService.TestServiceClient(channel);
            
            UnaryCall();
            ClientStreamingCall().Wait();
            ServerStreamingCall().Wait();
            DuplexStreamingCall().Wait();
        }

        private void UnaryCall()
        {
            PingResponse response = _client.UnaryPing(new PingRequest() { Value = 1 });
        }

        private async Task ClientStreamingCall()
        {
            using (AsyncClientStreamingCall<PingRequest, PingResponse> call = _client.ClientStreamingPing())
            {
                await call.RequestStream.WriteAsync(new PingRequest() {Value = 1});
                await call.RequestStream.CompleteAsync();
                PingResponse response = await call.ResponseAsync;
            }
        }

        private async Task ServerStreamingCall()
        {
            using (AsyncServerStreamingCall<PingResponse> call =
                _client.ServerStreamingPing(new PingRequest() {Value = 1}))
            {
                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    PingResponse response = call.ResponseStream.Current;
                }
            }
        }

        private async Task DuplexStreamingCall()
        {
            using (AsyncDuplexStreamingCall<PingRequest, PingResponse> call = _client.DuplexPing())
            {
                Task responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        PingResponse response = call.ResponseStream.Current;
                    }
                });

                await call.RequestStream.WriteAsync(new PingRequest() {Value = 1});
                await call.RequestStream.WriteAsync(new PingRequest() {Value = 1});
                await call.RequestStream.CompleteAsync();

                await responseReaderTask;
            }
        }
    }
}
