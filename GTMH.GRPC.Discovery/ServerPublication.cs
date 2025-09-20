using GTMH.Rabbit;
using GTMH.Rabbit.Impl;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public class ServerPublication
  {
    private readonly ILogger Log;
    private readonly string[] m_Addresses;
    private readonly IRabbitFactory Rabbit;
    private readonly string DiscoverableType;
    public ServerPublication(string a_DiscoverableType, ILogger a_Log, IRabbitFactory a_Rabbit, string a_AddressRequired, params string[] a_AddressOptional)
    {
      this.DiscoverableType = a_DiscoverableType;
      this.Log = a_Log;
      m_Addresses = new[] { a_AddressRequired }.Concat(a_AddressOptional).ToArray();
      this.Rabbit = a_Rabbit;
    }

    internal async Task<IAsyncDisposable> Register(CancellationToken stoppingToken)
    {
      var sinkFactory=RabbitStreamSinkFactory.Create(Rabbit, TransientQueueTopology.Create<DiscoveryResponse>());
      var sink = await sinkFactory.CreateSink(stoppingToken);
      IMessageStreamSource<DiscoveryRequest> ? source = null;
      Impl ? rval = null;
      try
      {
        var sourceFactory=RabbitStreamSourceFactory.Create(Rabbit, TransientQueueTopology.Create<DiscoveryRequest>());
        source = await sourceFactory.CreateSource(stoppingToken);

        try
        {
          rval = new Impl(DiscoverableType, m_Addresses, sink, source);
          await source.AddListenerAsync(DiscoverableType, rval);
        }
        catch(Exception)
        {
          rval = null;
          throw;
        }
        return rval;
      }
      finally
      {
        if(rval == null)
        {
          if ( source != null ) await source.DisposeAsync();
          await sink.DisposeAsync();
        }
      }
    }

    class Impl : IAsyncDisposable, IMessageStreamListener<DiscoveryRequest>
    {
      private readonly IMessageStreamSink<DiscoveryResponse> m_Sink;
      private readonly IMessageStreamSource<DiscoveryRequest> m_Source;
      private readonly string [] Addresses;
      private readonly string DiscoverableType;

      public Impl(string a_DiscoverableType, string [] a_Addresses, IMessageStreamSink<DiscoveryResponse> sink, IMessageStreamSource<DiscoveryRequest> source)
      {
        DiscoverableType = a_DiscoverableType;
        this.Addresses=a_Addresses;
        this.m_Sink = sink;
        this.m_Source = source;
      }

      public async ValueTask DisposeAsync()
      {
        await m_Source.DisposeAsync();
        await m_Sink.DisposeAsync();
      }

      public async ValueTask OnReceivedAsync(DiscoveryRequest a_Msg)
      {
        foreach(var address in this.Addresses)
        {
          await m_Sink.PublishAsync(DiscoverableType, new DiscoveryResponse(address));
        }
      }
    }
  }
}
