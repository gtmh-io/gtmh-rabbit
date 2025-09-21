using GTMH.GRPC.Discovery.AddressResolution;
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
    private readonly ILogger Log;
    private readonly IAddressResolver AddressResolution;
    public IRabbitFactory Rabbit { get; private set; }
    public abstract string DiscoverableType { get; }
    private readonly TimeSpan StartTimeout;

    public Discoverable(IServer a_Server, IHostApplicationLifetime a_HAL, IOptions<DiscoveryConfig> a_Config, ILogger<IDiscoveryService<T>> a_Log, IDecryptor a_Decryptor) : this(a_Server, a_HAL, a_Config, a_Log, a_Decryptor, new NoAddressResolution()) { }
    public Discoverable(IServer a_Server, IHostApplicationLifetime a_HAL, IOptions<DiscoveryConfig> a_Config, ILogger<IDiscoveryService<T>> a_Log, IDecryptor a_Decryptor, IAddressResolver a_Resolver)
    {
      this.Server = a_Server;
      this.HAL = a_HAL;
      this.Log=a_Log;
      this.AddressResolution = a_Resolver;
      Rabbit = new RabbitFactory(a_Config.Value.Transport, a_Decryptor);
      StartTimeout = TimeSpan.FromMilliseconds(a_Config.Value.StartTimeout);
    }

    public async Task<IAsyncDisposable> Publish(CancellationToken stoppingToken)
    {
      // TODO a timeout
      var tcs = new TaskCompletionSource();
      HAL.ApplicationStarted.Register(() => tcs.TrySetResult());
      await tcs.Task.WaitAsync(StartTimeout, stoppingToken);
      var saf = Server.Features.Get<IServerAddressesFeature>();
      if(saf == null) throw new DiscoveryException("Failed to find IServerAddressesFeature");
      else if(saf.Addresses == null) throw new DiscoveryException("Failed to find published addresses");
      else if(!saf.Addresses.Any()) throw new DiscoveryException("Server has empty addresses");

      var addresses = saf.Addresses.Select(_=>AddressResolution.Resolve(_)).Distinct();

      var first = addresses.First();
      var rval = new ServerPublication(this.DiscoverableType, Log, Rabbit, first, addresses.Skip(1).ToArray());

      return await rval.Register(stoppingToken);
    }
    protected class Listener : IMessageStreamListener<DiscoveryResponse>
    {
      public TaskCompletionSource<string[]> URI { get; private set; } = new TaskCompletionSource<string[]>(TaskCreationOptions.RunContinuationsAsynchronously);
      public ValueTask OnReceivedAsync(DiscoveryResponse a_Msg)
      {
        URI.SetResult(a_Msg.URI);
        return ValueTask.CompletedTask;
      }
    }
    private class NullLog : ILogger
    {
      public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>null;

      public bool IsEnabled(LogLevel logLevel) => false;
      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
  }
}
