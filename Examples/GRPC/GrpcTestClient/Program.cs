using Grpc.Net.Client;
using GrpcCommon;

using GTMH.Rabbit.Impl;

using var cts = new CancellationTokenSource(10000);
var uri = await HelloWorldDiscoverable.Locate(new HostOnlyFactory("localhost"), cts.Token );
if(uri == null)
{
  Console.WriteLine("Failed find server");
}
else
{
  // Create a channel
  using var channel = GrpcChannel.ForAddress(uri);
  var client = new HelloWorld.HelloWorldClient(channel);

  // Unary call
  var reply = await client.IntroducingAsync(new HelloRequest { Name = "World" });
  Console.WriteLine($"Response: {reply.Message}");
}




