using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

using DIRPC.Shared;

namespace DIRPC.Client;

public class Main : IAsyncDisposable
{
  private readonly ILogger<Main> Log;
  private HelloWorldClient ? m_Client = null;

  public Main(ILogger<Main> a_Log)
  {
    this.Log = a_Log;
  }


  public ValueTask<int> RunAsync(string[] args)
  {


    return ValueTask.FromResult(0);
  }

  public async ValueTask DisposeAsync()
  {
    if(m_Client != null)
    {
      await m_Client.DisposeAsync();
    }
  }
}

