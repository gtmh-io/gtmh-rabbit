
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

public class Worker : BackgroundService
{
  ILogger<Worker> Log;
  private readonly IServer Server;
  private readonly IHostApplicationLifetime HAL;

  public Worker(ILogger<Worker> a_Logger, IServer a_Server, IHostApplicationLifetime a_HAL)
  {
    Log = a_Logger;
    this.Server = a_Server;
    this.HAL = a_HAL;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var tcs = new TaskCompletionSource();
    HAL.ApplicationStarted.Register(()=>tcs.TrySetResult());
    await tcs.Task;
    var saf = Server.Features.GetRequiredFeature<IServerAddressesFeature>();
    foreach(var address in saf.Addresses)
    {
      Log.LogInformation($"Worker running at: {address} *************************");
    }


    while(!stoppingToken.IsCancellationRequested)
    {
      Log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
      try { await Task.Delay(1000, stoppingToken); } catch(TaskCanceledException) { }
    }
  }
}
