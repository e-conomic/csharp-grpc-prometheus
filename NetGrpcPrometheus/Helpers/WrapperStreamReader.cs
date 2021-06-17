using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace NetGrpcPrometheus.Helpers
{
    /// <summary>
    /// Wrapper for <see cref="IAsyncStreamReader{T}"/>. 
    /// Adds possibility to execute action after each message received from the stream.
    /// </summary>
    /// <typeparam name="T">Model object for message received from the stream</typeparam>
    public class WrapperStreamReader<T> : IAsyncStreamReader<T>
    {
        public T Current => _reader.Current;

        private readonly IAsyncStreamReader<T> _reader;
        private readonly Action _onMessage;
        private readonly Action<StatusCode> _onError;

        /// <summary>
        /// Constructor for <see cref="IAsyncStreamReader{T}"/> wrapper
        /// </summary>
        /// <param name="reader">Stream reader that should be wrapped by this class</param>
        /// <param name="onMessage">Action that should be executed on each message received from the stream</param>
        public WrapperStreamReader(IAsyncStreamReader<T> reader, Action onMessage, Action<StatusCode> onError)
        {
            _reader = reader;
            _onMessage = onMessage;
            _onError = onError;
        }

        public void Dispose()
        {

        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _reader.MoveNext(cancellationToken);
                _onMessage.Invoke();
                return result;
            }
            catch (RpcException e)
            {
                _onError(e.StatusCode);
                throw;
            }
        }
    }
}
