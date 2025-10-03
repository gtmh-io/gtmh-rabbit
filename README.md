Started out as a Rabbit Lib and evolved from there to three main components. In order of interest they are
- Full blown Rabbit RPC
- Discovery for gRPC services using Rabbit
- Typesafe Rabbit stream
# GTMH.RPC - Code-Generated RPC Client/Server for C#

A high-performance RPC (Remote Procedure Call) framework for C# that uses Roslyn code generation to create strongly-typed clients and servers with minimal boilerplate. 

## Features

- **Code Generation** - Roslyn-powered source generators create client/server code at compile time
- **Type Safety** - Full IntelliSense support and compile-time checking
- **Async First** - Built on `ValueTask` for optimal performance
- **Zero Reflection** - All serialization code is generated at compile time
- **Minimal Overhead** - Direct method calls feel like local invocations

Uses the excellent https://github.com/protobuf-net/protobuf-net serialisation library for object serialisation

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
###📦 Installation
```bash
dotnet add package GTMH.RPC
dotnet add package GTMH.RPC.CodeGen
