
using GrpcCommon;

using GTMH.GRPC.Discovery;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

public class Worker : BackgroundService, IAsyncDisposable
{
  ILogger<Worker> Log;
  private readonly IDiscoveryService<HelloWorld.HelloWorldClient> Discovery;
  private IAsyncDisposable ? m_Publication;

  public Worker(ILogger<Worker> a_Logger, IDiscoveryService<HelloWorld.HelloWorldClient> a_Discovery)
  {
    Log = a_Logger;
    this.Discovery = a_Discovery;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    m_Publication = await Discovery.Publish(stoppingToken);
    while(!stoppingToken.IsCancellationRequested)
    {
      Log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
      try { await Task.Delay(1000, stoppingToken); } catch(TaskCanceledException) { }
    }
  }

  public async ValueTask DisposeAsync()
  {
    if(m_Publication != null)
    {
      await m_Publication.DisposeAsync();
    }
  }
}
