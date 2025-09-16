using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.Impl
{
  public class RabbitStreamSinkFactory<M> : IMessageStreamSinkFactory<M>
  {
    RabbitInstance<M> m_Rabbit; 
    public RabbitStreamSinkFactory(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology, ILogger<RabbitStreamSinkFactory<M>> a_Logger)
    {
      m_Rabbit = new RabbitInstance<M>(a_Rabbit, a_Topology, a_Logger);
    }

    public async ValueTask<IMessageStreamSink<M>> CreateSink(CancellationToken a_Cancel = default)
    {
      var stream = await m_Rabbit.Connect(a_Cancel);
      return new Sink(stream);
    }

    class Sink : IMessageStreamSink<M>
    {
      private RabbitInstance<M>.StreamConnection<M> Connection;

      public Sink(RabbitInstance<M>.StreamConnection<M> stream)
      {
        this.Connection = stream;
      }

      public async ValueTask DisposeAsync()
      {
        await Connection.DisposeAsync();
      }

      public async ValueTask PublishAsync(string a_RoutingKey, M a_Msg, CancellationToken a_Cancel = default)
      {
        if ( string.IsNullOrEmpty(a_RoutingKey) ) throw new ArgumentException("Empty routing key is ambiguous");
        var payload = PBuffer.Create(a_Msg).Data;
        await Connection.Channel.BasicPublishAsync(Connection.Topology.ExchangeName, a_RoutingKey, mandatory:false, payload, a_Cancel);
      }
    }
  }
}
