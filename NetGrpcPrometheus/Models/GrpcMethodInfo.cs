using System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace NetGrpcPrometheus.Models
{
    public class GrpcMethodInfo
    {
        private readonly MethodType _type;

        public string ServiceName { get; }
        public string Name { get; }

        public string MethodType
        {
            get
            {
                switch (_type)
                {
                    case Grpc.Core.MethodType.Unary:
                        return "unary";
                    case Grpc.Core.MethodType.ClientStreaming:
                        return "client_stream";
                    case Grpc.Core.MethodType.ServerStreaming:
                        return "server_stream";
                    case Grpc.Core.MethodType.DuplexStreaming:
                        return "bidi_stream";
                    default:
                        return "unary";
                }
            }
        }

        public GrpcMethodInfo(string fullName, MethodType type)
        {
            string[] names = fullName.Split('/');
            _type = type;

            ServiceName = names[1];
            Name = names[2];
        }
    }
}
