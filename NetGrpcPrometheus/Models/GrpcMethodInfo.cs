using Grpc.Core;

namespace NetGrpcPrometheus.Models
{
    /// <summary>
    /// Class for handling information about gRPC calls
    /// </summary>
    public class GrpcMethodInfo
    {
        private readonly MethodType _type;

        /// <summary>
        /// Name of the service gRPC call is intented for
        /// </summary>
        public string ServiceName { get; }
        
        /// <summary>
        /// Name of the gRPC method called
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indicates call type of gRPC call
        /// </summary>
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

        /// <summary>
        /// Constructor for <see cref="GrpcMethodInfo"/>.
        /// Parses different information about gRPC call
        /// </summary>
        /// <param name="fullName">full name of gRPC call including service name and method name</param>
        /// <param name="type">Type of the gRPC call</param>
        public GrpcMethodInfo(string fullName, MethodType type)
        {
            string[] names = fullName.Split('/');
            _type = type;

            ServiceName = names[1];
            Name = names[2];
        }
    }
}
