using GrpcCommon;

using GrpcWorkerService;

using GTMH.GRPC.Discovery;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddGrpc();
builder.Services.AddHostedService<Worker>();
builder.AddDiscoveryConfig();
builder.Services.AddSingleton<IDiscoveryService<HelloWorld.HelloWorldClient>, HelloWorldDiscoverable>();
builder.Services.AddSingleton<ServerImpl>();

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(5001, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
});

var app = builder.Build();

// Configure request pipeline
app.MapGrpcService<ServerImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

var publication = await app.Services.GetRequiredService<ServerImpl>().Publish();
await using(publication)
{
  await app.RunAsync();
}
