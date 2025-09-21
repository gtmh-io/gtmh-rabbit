using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.Impl
{
  public class RabbitStreamSinkFactory
  {
    public static IMessageStreamSinkFactory<M> Create<M>(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology)
    {
      return new RabbitStreamSinkFactory_t<M>(a_Rabbit, a_Topology);
    }
    public static IMessageStreamSinkFactory<M> Create<M>(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology, ILogger a_Logger)
    {
      return new RabbitStreamSinkFactory_t<M>(a_Rabbit, a_Topology, a_Logger);
    }
  }
  public class RabbitStreamSinkFactory_t<M> : IMessageStreamSinkFactory<M>
  {
    RabbitInstance<M> m_Rabbit; 
    public RabbitStreamSinkFactory_t(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology, ILogger<RabbitStreamSinkFactory_t<M>> a_Logger)
    {
      m_Rabbit = new RabbitInstance<M>(a_Rabbit, a_Topology, a_Logger);
    }
    public RabbitStreamSinkFactory_t(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology, ILogger a_Logger)
    {
      m_Rabbit = new RabbitInstance<M>(a_Rabbit, a_Topology, a_Logger);
    }
    public RabbitStreamSinkFactory_t(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology)
    {
      m_Rabbit = new RabbitInstance<M>(a_Rabbit, a_Topology, new NullLogger());
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
        var payload = PBuffer.Create(a_Msg).Data;
        await Connection.Channel.BasicPublishAsync(Connection.Topology.ExchangeName, a_RoutingKey, mandatory:false, payload, a_Cancel);
      }
    }
    class NullLogger : ILogger
    {
      public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>null;
      public bool IsEnabled(LogLevel logLevel) => false;
      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
  }
}
