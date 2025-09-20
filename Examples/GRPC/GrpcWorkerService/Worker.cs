
using GrpcCommon;

using GTMH.GRPC.Discovery;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

public class Worker : BackgroundService
{
  ILogger<Worker> Log;
  private readonly IDiscoveryService<HelloWorld.HelloWorldClient> Discovery;

  public Worker(ILogger<Worker> a_Logger, IDiscoveryService<HelloWorld.HelloWorldClient> a_Discovery)
  {
    Log = a_Logger;
    this.Discovery = a_Discovery;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {

    var pub = await Discovery.Publish();
    /*var tcs = new TaskCompletionSource();
    Discovery.HAL.ApplicationStarted.Register(() => tcs.TrySetResult());
    await tcs.Task;
    var saf = Discovery.Server.Features.Get<IServerAddressesFeature>();

    if(saf == null) throw new DiscoveryException("Failed to find IServerAddressesFeature");
    else if(saf.Addresses == null) throw new DiscoveryException("Failed to find published addresses");
    else if(!saf.Addresses.Any()) throw new DiscoveryException("Server has empty addresses");
    var first = saf.Addresses.First();*/

    while(!stoppingToken.IsCancellationRequested)
    {
      Log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
      try { await Task.Delay(1000, stoppingToken); } catch(TaskCanceledException) { }
    }
  }
}
