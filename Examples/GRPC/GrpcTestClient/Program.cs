using Grpc.Net.Client;
using GrpcCommon;

// Create a channel
using var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new HelloWorld.HelloWorldClient(channel);

// Unary call
var reply = await client.IntroducingAsync(new HelloRequest { Name = "World" });
Console.WriteLine($"Response: {reply.Message}");


