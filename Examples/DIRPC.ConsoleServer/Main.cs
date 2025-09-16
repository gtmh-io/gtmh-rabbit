using GTMH.Rabbit.RPC;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

namespace DIRPC.ConsoleServer;

public class Main : IAsyncDisposable
{
  private readonly ILogger<Main> Log;
  public Main(ILogger<Main> a_Log, IRPCFactory a_RPCFactory)
  {
    this.Log = a_Log;
  }

  public ValueTask<int> RunAsync()
  {
    Log.LogInformation("Server is running...");


    return ValueTask.FromResult(0);
  }

  public ValueTask DisposeAsync()
  {
    return ValueTask.CompletedTask;
  }
}
