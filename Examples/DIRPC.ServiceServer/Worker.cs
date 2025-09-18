using DIRPC.Shared;

using GTMH.Rabbit.RPC;

namespace DIRPC.ServiceServer;

public class Worker : BackgroundService
{
  public class ServerImpl : IHelloWorld
  {
    public ValueTask<string> IntroducingAsync(string a_Identity) => ValueTask.FromResult($"Hello {a_Identity}, I am a GTMH.RPC.HelloWorldServer.Service");
  }
  ILogger<Worker> Log;
  private HelloWorldServiceHost m_ServiceHost;

  public Worker(ILogger<Worker> a_Logger, IRPCFactory a_RPCFactory)
  {
    Log = a_Logger;
    Log.LogInformation($"Server running against rabbit@{a_RPCFactory.Transport.HostIdentity}");
    m_ServiceHost = new HelloWorldServiceHost(a_RPCFactory, new ServerImpl(), Log);
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var server = await m_ServiceHost.Publish();
    await using(server)
    {
      while(!stoppingToken.IsCancellationRequested)
      {
        Log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        try { await Task.Delay(1000, stoppingToken); } catch(TaskCanceledException) { }
      }
    }
  }
}
