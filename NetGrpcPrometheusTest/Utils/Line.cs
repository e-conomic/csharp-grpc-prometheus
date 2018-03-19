namespace NetGrpcPrometheusTest.Utils
{
    public class Line
    {
        public string MetricsName { get; }
        public string Type { get; }
        public string ServiceName => "NetGrpcPrometheusTest.Grpc.TestService";
        public string MethodName { get; }
        public string StatusCode { get; set; }

        public Line(string line)
        {
            if (!line.StartsWith("grpc"))
            {
                return;
            }

            MetricsName = line.Split("{")[0];
            string content = line.Split("{")[1].Split("}")[0];
            string[] attributes = content.Split(",");

            foreach (string attribute in attributes)
            {
                if (attribute.Contains("grpc_type"))
                {
                    Type = attribute.Split("=")[1].Replace("\"", "");
                }
                else if (attribute.Contains("grpc_method"))
                {
                    MethodName = attribute.Split("=")[1].Replace("\"", "");
                }
                else if (attribute.Contains("grpc_code"))
                {
                    StatusCode = attribute.Split("=")[1].Replace("\"", "");
                }
            }
        }
    }
}
