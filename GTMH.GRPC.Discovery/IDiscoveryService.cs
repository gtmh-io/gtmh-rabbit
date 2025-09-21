using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public interface IDiscoveryService<T>
  {
    Task<IAsyncDisposable> Publish(CancellationToken stoppingToken);
  }
}
