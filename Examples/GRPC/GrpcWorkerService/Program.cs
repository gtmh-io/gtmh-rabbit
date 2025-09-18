using GrpcWorkerService;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddHostedService<Worker>();

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(0, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
});

var app = builder.Build();

// Configure request pipeline
app.MapGrpcService<ServerImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

if(app.Environment.IsDevelopment())
{
  app.MapGrpcReflectionService();
}

await app.RunAsync();
