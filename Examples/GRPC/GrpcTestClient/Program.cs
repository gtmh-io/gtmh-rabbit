

using Grpc.Net.Client;
using GrpcWorkerService;

// Create a channel
using var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new Greeter.GreeterClient(channel);

// Unary call
var reply = await client.SayHelloAsync(new HelloRequest { Name = "World" });
Console.WriteLine($"Response: {reply.Message}");


