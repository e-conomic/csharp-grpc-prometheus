using System.Collections.Generic;
using System.Net;

namespace NetGrpcPrometheusTest.Utils
{
    public class TestHelper
    {
        public static readonly string UnaryTypeName = "unary";
        public static readonly string ClientStreamingTypeName = "client_stream";
        public static readonly string ServerStreamingTypeName = "server_stream";
        public static readonly string DuplexStreamingTypeName = "bidi_stream";
        
        public static List<Line> GetLines(string hostname, int port)
        {
            List<Line> lines = new List<Line>();
            string content;

            using (WebClient webClient = new WebClient())
            {
                content = webClient.DownloadString($"http://{hostname}:{port}/metrics");
            }

            string[] rawLines = content.Split('\r', '\n');

            foreach (string rawLine in rawLines)
            {
                Line line = new Line(rawLine);
                if (!string.IsNullOrEmpty(line.MetricsName))
                {
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}
