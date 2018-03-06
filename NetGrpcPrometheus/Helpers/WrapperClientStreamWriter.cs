using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace NetGrpcPrometheus.Helpers
{
    public class WrapperClientStreamWriter<T> : IClientStreamWriter<T>
    {
        private readonly IClientStreamWriter<T> _writer;
        private readonly Action _onMessage;

        public WrapperClientStreamWriter(IClientStreamWriter<T> writer, Action onMessage)
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
        
        public Task CompleteAsync()
        {
            return _writer.CompleteAsync();
        }
    }
}
