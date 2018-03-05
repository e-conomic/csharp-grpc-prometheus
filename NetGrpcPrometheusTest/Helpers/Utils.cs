using System;
using System.Globalization;
using System.Net;

namespace NetGrpcPrometheusTest.Helpers
{
    public class Utils
    {
        private static string _content;
        
        public static bool ContainsMetric(string metricName, string callType, string methodName)
        {
            if (String.IsNullOrEmpty(_content))
            {
                using (WebClient webClient = new WebClient())
                {
                    _content = webClient.DownloadString($@"http://{TestServer.MetricsHostname}:{TestServer.MetricsPort}/metrics");
                }
            }

            return _content.Contains(
                $"{metricName}{{grpc_type=\"{callType}\",grpc_service=\"{TestServer.ServiceName}\",grpc_method=\"{methodName}");
        }
    }
}
