using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

using DIRPC.Shared;
using GTMH.Rabbit.RPC;
using Microsoft.Extensions.Options;

namespace DIRPC.Client;

public class Main : IAsyncDisposable
{
  private readonly ILogger<Main> Log;
  private HelloWorldClient m_Client;

  public Main(ILogger<Main> a_Log, IRPCFactory a_RPCFactory, IOptions<RPCClientConfig> a_ClientConfig)
  {
    this.Log = a_Log;
    m_Client=new HelloWorldClient(a_RPCFactory, a_Log, a_ClientConfig.Value);
  }


  public async ValueTask<int> RunAsync()
  {
    try
    {
      await m_Client.Connect();
      var result = await m_Client.IntroducingAsync("client");
      Log.LogInformation($"RPC pleasantries... {result}");
    }
    catch(RPCTimeout e)
    {
      Log.LogError(e, "Is your server running?");
      return -1;
    }
    return 0;
  }

  public async ValueTask DisposeAsync()
  {
    await m_Client.DisposeAsync();
  }
}

