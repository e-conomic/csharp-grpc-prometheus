using System;
using System.Net;

namespace NetGrpcPrometheusTest.Helpers
{
    public class Utils
    {
        public static bool ContainsMetric(string metricName, string callType, string methodName, string grpcCode, string hostname,
            int port)
        {
            string content;

            using (WebClient webClient = new WebClient())
            {
                content = webClient.DownloadString($@"http://{hostname}:{port}/metrics");
            }
            
            // e.g.:
            // grpc_client_handled_total{grpc_type="unary",grpc_service="PrometheusTest.Grpc.MathService",grpc_method="ThanksWelcome",grpc_code="OK"}

            if (String.IsNullOrEmpty(grpcCode))
            {
                return content.Contains(
                    $"{metricName}{{grpc_type=\"{callType}\",grpc_service=\"{TestServer.ServiceName}\",grpc_method=\"{methodName}");
            }

            return content.Contains(
                $"{metricName}{{grpc_type=\"{callType}\",grpc_service=\"{TestServer.ServiceName}\",grpc_method=\"{methodName}\",grpc_code=\"{grpcCode}\"");
        }
    }
}
