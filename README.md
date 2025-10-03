Started out as a Rabbit Lib and evolved from there to three main components. In order of interest they are
- Full blown Rabbit RPC
- Discovery for gRPC services using Rabbit
- Typesafe Rabbit stream
# GTMH.Rabbit.RPC - Code-Generated RPC Client/Server for C#

A high-performance RPC (Remote Procedure Call) framework for C# that uses Roslyn code generation to create strongly-typed clients and servers with minimal boilerplate. Behind the scenes it uses the mature https://www.rabbitmq.com/client-libraries/dotnet for transport and https://github.com/protobuf-net/protobuf-net for serialisation

## Features

- **Code Generation** - Roslyn-powered source generators create client/server code at compile time
- **Type Safety** - Full IntelliSense support and compile-time checking
- **Async First** - Built on `ValueTask` for optimal performance
- **Zero Reflection** - All serialization code is generated at compile time
- **Minimal Overhead** - Direct method calls feel like local invocations

## Quick Start

### 1. Define Your RPC Interface
```csharp
[RPCInterface]
interface IHelloWorld
{
    [RPCMethod]
    ValueTask<string> IntroducingAsync(string a_Identity);
}
```
### 2. Implement the Server
```csharp
public class ServerImpl : IHelloWorld
{
    public ValueTask<string> IntroducingAsync(string a_Identity) 
        => ValueTask.FromResult($"Hello {a_Identity}, I am a GTMH.RPC.HelloWorldServer");
}
```
### 3. Run Client and Server
```csharp
// Server
var serverHost = new HelloWorldServiceHost(rpcFactory, new ServerImpl());
var server = await serverHost.Publish();

// Client
var client = new HelloWorldClient(rpcFactory);
await client.Connect();
var result = await client.IntroducingAsync("client");
Console.WriteLine($"RPC pleasantries... {result}");
```
### 📦 Installation
```bash
dotnet add package GTMH.Rabbit.RPC
dotnet add package GTMH.Rabbit.RPC.CodeGen
```
### The Elephant in the Room - gRPC
Why not use gRPC? 

Firstly, GTMH.Rabbit.RPC is for c# only so, if you want cross language RPC then gRPC is your guy.

For those on c# I'd say, firstly, that GTMH.Rabbit.RPC is configuration and specification lite. In code there is minimal markup and it is inline via attributes on the server with no need for separate IDL files. At runtime, for those already using RabbitMQ there is no additional configuration overhead at all. You simply piggy back of your existing rabbit server technology with the robustness and features it provides. At runtime three is no need, as in for gRPC, for the dissemination of multiple hostname and port mappings on the client side. RPC services are in a sense discoverable by virtue of the fact that they exist in the rabbit ecosystem. Similarly, thee's no need for management of firewall rules across client/server machines

As a minor point, GTMH.Rabbit.RPC on the server side is not tied to ASP.net and related dependency injection infrastructure. Given a server implementation it can be up and running in two lines of code in a console app as compared to dozens for setup and configuration of gRPC in ASP.net. If you want to run your RPC server as a systemd or Windows Service there's no need for the coding contortions otherwise required with ASP.net and BackgroundService.

From an esoteric point of view, as opposed to gRPC, the server doesn't know it's a server. In gRPC the server implementation is strongly tied to it's server-ness through inheritance. Here, the server simply implements an interface and RPC is optional
