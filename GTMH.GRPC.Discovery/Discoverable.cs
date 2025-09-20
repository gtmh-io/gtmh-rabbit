using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public abstract class Discoverable<T> : IDiscoveryService<T>
  {
    private readonly IServer Server;
    private readonly IHostApplicationLifetime HAL;

    public abstract string DiscoverableType { get; }

    public Discoverable(IServer a_Server, IHostApplicationLifetime a_HAL, IOptions<DiscoveryConfig> a_Config)
    {
      this.Server = a_Server;
      this.HAL = a_HAL;
    }

    public async Task<IAsyncDisposable> Publish()
    {
      // TODO a timeout
      var tcs = new TaskCompletionSource();
      HAL.ApplicationStarted.Register(()=>tcs.TrySetResult());
      await tcs.Task;
      var saf = Server.Features.Get<IServerAddressesFeature>();
      if ( saf ==null ) throw new DiscoveryException("Failed to find IServerAddressesFeature");
      else if ( saf.Addresses == null ) throw new DiscoveryException("Failed to find published addresses");
      else if ( ! saf.Addresses.Any() ) throw new DiscoveryException("Server has empty addresses");
      var first = saf.Addresses.First();
      var rval = new ServerPublication(first, saf.Addresses.Skip(1).ToArray());
      await rval.Publish();
      return rval;
    }
  }
}
