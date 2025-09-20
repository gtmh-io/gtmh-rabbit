using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public class ServerPublication : IAsyncDisposable
  {
    public ServerPublication(string a_AddressRequired, params string[] a_AddressOptional)
    {
    }

    public ValueTask DisposeAsync()
    {
      return ValueTask.CompletedTask;
    }

    internal Task Publish()
    {
      return Task.CompletedTask;
    }
  }
}
