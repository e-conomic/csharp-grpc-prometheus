# C# gRPC interceptors for Prometheus monitoring
[Prometheus](https://prometheus.io/) monitoring for [gRPC C#](https://github.com/grpc/grpc) servers and clients.

This project was inspired by [grpc-ecosystem/go-grpc-prometheus](https://github.com/grpc-ecosystem/go-grpc-prometheus) 
and [grpc-ecosystem/java-grpc-prometheus](https://github.com/grpc-ecosystem/java-grpc-prometheus)

Library is build in .NET standart 2.0 and uses [prometheus-net](https://github.com/prometheus-net/prometheus-net) for 
Prometheus metrics handling.

## Usage
You can set up client-side or server-side interceptor.

### Server-side

```C#
ServerInterceptor interceptor = new ServerInterceptor(hostname: "127.0.0.1", port: "1234");

Server server = new Server();
server.Services.Add(TestService.BindService(new TestServiceImp()).Intercept(interceptor));
server.Ports.Add(new ServerPort(host: "127.0.0.1", port: 50051, credentials: ServerCredentials.Insecure));
server.Start();
```

### Client-side

```C#
ClientInterceptor interceptor = new ClientInterceptor(hostname: "127.0.0.1", port: "1234");

Channel channel = new Channel(host: "127.0.0.1", port: 50051, credentials: ChannelCredentials.Insecure);
TestService.TestServiceClient client = new TestService.TestServiceClient(channel.Intercept(interceptor));
```

## Metrics

Metrics can be found on hostname and port specified in constructor for `ServerInterceptor` 
or `ClientInterceptor` under the `/metrics` endpoint.

### Labels

All server-side metrics start with `grpc_server` as Prometheus subsystem name. All client-side metrics start with `grpc_client`. 
Both of them have mirror-concepts. Similarly all methods contain the same rich labels:

* `grpc_service` - the gRPC service name, which is the combination of `protobuf` package and the `grpc_service` section name.
E.g. for `package NetGrpcPrometheusTest;` and service `TestService` the label will be `grpc_service="NetGrpcPrometheusTest.TestService"`
* `grpc_method` - the name of the method called on the gRPC service. E.g.
`grpc_method="Ping"`
* `grpc_type` - the gRPC [type of request](https://grpc.io/docs/guides/concepts.html#rpc-life-cycle) 

Additionally for completed RPCs, the following labels are used:

* `grpc_code` - the human-readable [gRPC status code](https://github.com/grpc/grpc-go/blob/master/codes/codes.go)

### Counters

There are four types of counters defined for both client and server side:

#### Server-side

* `grpc_server_started_total` - counts total calls started on the server
* `grpc_server_handled_total` - counts total calls handled and sent back to client from the server
* `grpc_server_msg_received_total` - counts total number of messages received through the streams
* `grpc_server_msg_sent_total` - counts total number of messages sent through the streams

#### Client-side

* `grpc_client_started_total` - counts total calls sent to the server
* `grpc_client_handled_total` - counts total calls received from the server
* `grpc_client_msg_received_total` - counts total number of messages received through the streams
* `grpc_client_msg_sent_total` - counts total number of messages sent through the streams

For more detailed documention go to (MetricsBase.cs)[NetGrpcPrometheus/Helpers/MetricsBase.cs]

### Histograms

[Prometheus histograms](https://prometheus.io/docs/concepts/metric_types/#histogram) are a great way to measure latency distributions of your RPCs. 
However since it is bad practice to have metrics of [high cardinality](https://prometheus.io/docs/practices/instrumentation/#do-not-overuse-labels)) the latency monitoring metrics are disabled by default. 
To enable them please call the following in your server initialization code:

```C#
ServerInterceptor interceptor = new ServerInterceptor(hostname: "127.0.0.1", port: "1234");
interceptor.EnableLatencyMetrics = true;
```

After the call completes, it's handling time will be recorded in a [Prometheus histogram](https://prometheus.io/docs/concepts/metric_types/#histogram) variable 
grpc_server_handling_seconds. It contains three sub-metrics:

* `grpc_server_handling_seconds_count` - the count of all completed RPCs by status and method
* `grpc_server_handling_seconds_sum` - cumulative time of RPCs by status and method, useful for calculating average handling times
* `grpc_server_handling_seconds_bucket` - contains the counts of RPCs by status and method in respective handling-time buckets

## Custom Metrics

For custom metrics follow guidelines on [prometheus-net](https://github.com/prometheus-net/prometheus-net). Please note that [MetricsServer](https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.NetStandard/MetricServer.cs) is already running so you can jump directly to creating the metrics.

## Useful query examples

Please find them on [grpc-ecosystem/go-grpc-prometheus](https://github.com/grpc-ecosystem/go-grpc-prometheus)

## License

Please find the license [here](LICENSE)

