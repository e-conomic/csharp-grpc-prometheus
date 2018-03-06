﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace NetGrpcPrometheus.Helpers
{
    public class WrapperStreamReader<T> : IAsyncStreamReader<T>
    {
        private readonly IAsyncStreamReader<T> _reader;
        private readonly Action _onMessage;

        public WrapperStreamReader(IAsyncStreamReader<T> reader, Action onMessage)
        {
            _reader = reader;
            _onMessage = onMessage;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            Task<bool> result = _reader.MoveNext(cancellationToken);

            result.ContinueWith(task =>
            {
                _onMessage.Invoke();
            }, cancellationToken);

            return result;
        }

        public T Current => _reader.Current;
    }
}