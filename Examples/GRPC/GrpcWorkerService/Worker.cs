
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

public class Worker : BackgroundService
{
  ILogger<Worker> Log;

  public Worker(ILogger<Worker> a_Logger)
  {
    Log = a_Logger;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while(!stoppingToken.IsCancellationRequested)
    {
      Log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
      try { await Task.Delay(1000, stoppingToken); } catch(TaskCanceledException) { }
    }
  }
}
