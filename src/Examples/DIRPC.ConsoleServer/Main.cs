using DIRPC.Shared;

using GTMH.Rabbit.RPC;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

namespace DIRPC.ConsoleServer;

public class Main
{
  public class ServerImpl : IHelloWorld
  {
    public ValueTask<string> IntroducingAsync(string a_Identity) => ValueTask.FromResult($"Hello {a_Identity}, I am a GTMH.RPC.HelloWorldServer.Console");
  }

  private readonly ILogger<Main> Log;
  HelloWorldServiceHost m_ServiceHost;
  public Main(ILogger<Main> a_Log, IRPCFactory a_RPCFactory)
  {
    this.Log = a_Log;
    Log.LogInformation($"Server running against rabbit@{a_RPCFactory.Transport.HostIdentity}");
    m_ServiceHost = new HelloWorldServiceHost(a_RPCFactory, new ServerImpl(), a_Log);
  }

  public async ValueTask<int> RunAsync()
  {
    var server = await m_ServiceHost.Publish();
    await using(server)
    {
      Log.LogInformation("Server is running, press enter to shutdown");
      await Task.Run(()=>Console.ReadLine());

    }
    return 0;
  }
}
