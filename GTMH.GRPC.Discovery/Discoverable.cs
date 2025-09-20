using GTMH.Rabbit;
using GTMH.Security;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public abstract class Discoverable<T> : IDiscoveryService<T>
  {
    public IServer Server { get; private set; }
    public IHostApplicationLifetime HAL { get; private set; }
    private readonly ILogger<IDiscoveryService<T>> Log;
    private readonly IRabbitFactory m_Rabbit;

    public abstract string DiscoverableType { get; }

    public Discoverable(IServer a_Server, IHostApplicationLifetime a_HAL, IOptions<DiscoveryConfig> a_Config, ILogger<IDiscoveryService<T>> a_Log, IDecryptor a_Decryptor)
    {
      this.Server = a_Server;
      this.HAL = a_HAL;
      this.Log=a_Log;
      m_Rabbit = new RabbitFactory(a_Config.Value.Transport, a_Decryptor);
    }

    public async Task<IAsyncDisposable> Publish(CancellationToken stoppingToken)
    {
      // TODO a timeout
      var tcs = new TaskCompletionSource();
      HAL.ApplicationStarted.Register(() => tcs.TrySetResult());
      await tcs.Task;
      var saf = Server.Features.Get<IServerAddressesFeature>();
      if(saf == null) throw new DiscoveryException("Failed to find IServerAddressesFeature");
      else if(saf.Addresses == null) throw new DiscoveryException("Failed to find published addresses");
      else if(!saf.Addresses.Any()) throw new DiscoveryException("Server has empty addresses");

      var first = saf.Addresses.First();
      var rval = new ServerPublication(this.DiscoverableType, Log, m_Rabbit, first, saf.Addresses.Skip(1).ToArray());

      return await rval.Register(stoppingToken);
    }
  }
}
