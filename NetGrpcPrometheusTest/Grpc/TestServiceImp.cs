using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace NetGrpcPrometheusTest.Grpc
{
    public class TestServiceImp : TestService.TestServiceBase
    {
        public override Task<PingResponse> UnaryPing(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse() { Value = 1 });
        }

        public override async Task<PingResponse> ClientStreamingPing(IAsyncStreamReader<PingRequest> requestStream, ServerCallContext context)
        {
            int i = 0;

            while (await requestStream.MoveNext(CancellationToken.None))
            {
                i++;
            }

            return new PingResponse() { Value = i };
        }

        public override async Task ServerStreamingPing(PingRequest request, IServerStreamWriter<PingResponse> responseStream, ServerCallContext context)
        {
            for (int i = 0; i < request.Value; i++)
            {
                await responseStream.WriteAsync(new PingResponse() { Value = 1 });
            }
        }

        public override async Task DuplexPing(IAsyncStreamReader<PingRequest> requestStream, IServerStreamWriter<PingResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext(CancellationToken.None))
            {
                await responseStream.WriteAsync(new PingResponse() { Value = 1 });
            }
        }
    }
}
