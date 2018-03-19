using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace NetGrpcPrometheusTest.Grpc
{
    public class TestServiceAsyncImp : TestService.TestServiceBase
    {
        public override async Task<PingResponse> UnaryPing(PingRequest request, ServerCallContext context)
        {
            if (request.Status == Status.Ok)
            {
                PingResponse response = null;

                await Task.Run(() => response = new PingResponse() {Message = "OK"});
                
                return response;
            }

            global::Grpc.Core.Status status = new global::Grpc.Core.Status(StatusCode.Internal, "details");
            throw new RpcException(status);
        }

        public override async Task<PingResponse> ClientStreamingPing(IAsyncStreamReader<PingRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext(CancellationToken.None))
            {
                if (requestStream.Current.Status == Status.Bad)
                {
                    ThrowException();
                }
            }

            return new PingResponse() { Message = "OK" };
        }

        public override async Task ServerStreamingPing(PingRequest request, IServerStreamWriter<PingResponse> responseStream, ServerCallContext context)
        {
            if (request.Status == Status.Bad)
            {
                ThrowException();
            }

            for (int i = 0; i < 1; i++)
            {
                await responseStream.WriteAsync(new PingResponse() { Message = "OK" });
            }
        }

        public override async Task DuplexPing(IAsyncStreamReader<PingRequest> requestStream, IServerStreamWriter<PingResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext(CancellationToken.None))
            {
                if (requestStream.Current.Status == Status.Bad)
                {
                    ThrowException();
                }

                await responseStream.WriteAsync(new PingResponse() { Message = "OK" });
            }
        }

        private void ThrowException()
        {
            global::Grpc.Core.Status status = new global::Grpc.Core.Status(StatusCode.Internal, "details");
            throw new RpcException(status);
        }
    }
}
