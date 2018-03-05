using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace NetGrpcPrometheus.Helpers
{
    public class WrapperServerStreamWriter<T> : IServerStreamWriter<T>
    {
        private readonly IServerStreamWriter<T> _writer;
        private readonly Action _onMessage;

        public WrapperServerStreamWriter(IServerStreamWriter<T> writer, Action onMessage)
        {
            _writer = writer;
            _onMessage = onMessage;
        }

        public Task WriteAsync(T message)
        {
            Task result = _writer.WriteAsync(message);

            result.ContinueWith(task =>
            {
                _onMessage.Invoke();
            });

            return result;
        }

        public WriteOptions WriteOptions
        {
            get => _writer.WriteOptions;
            set => _writer.WriteOptions = value;
        }
    }
}
