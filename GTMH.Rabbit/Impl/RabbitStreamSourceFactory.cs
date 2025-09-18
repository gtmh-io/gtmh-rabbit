using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.Impl
{
  public class RabbitStreamSourceFactory<M> : IMessageStreamSourceFactory<M> 
  {
    RabbitInstance<M> m_Rabbit;
    private readonly ILogger<RabbitStreamSourceFactory<M>> Log;

    public RabbitStreamSourceFactory(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology, ILogger<RabbitStreamSourceFactory<M>> ? a_Logger = null)
    {
      var logger = a_Logger ?? new NullStreamFactoryLogger();
      m_Rabbit = new RabbitInstance<M>(a_Rabbit, a_Topology, logger);
      this.Log=logger;
    }

    public async ValueTask<IMessageStreamSource<M>> CreateSource(CancellationToken a_Cancel = default)
    {
      var stream = await m_Rabbit.Connect(a_Cancel);
      return new Source(stream, Log, m_Rabbit.Topology);
    }

    class Source : IMessageStreamSource<M>
    {
      private readonly IMQTopology<M> Topology;
      private RabbitInstance<M>.StreamConnection<M> Connection;
      private readonly ILogger Log;
      ConcurrentDictionary<IMessageStreamListener<M>, string> m_Listeners = new();
      public Source(RabbitInstance<M>.StreamConnection<M> stream, ILogger a_Log, IMQTopology<M> a_Topology)
      {
        this.Topology = a_Topology;
        this.Connection = stream;
        this.Log=a_Log;
      }

      static string RoutingKey(string? a_RoutingKey)
      {
        if(a_RoutingKey == null)
        {
          return "#";
        }
        else
        {
          if(a_RoutingKey == "") return "#";
          else return a_RoutingKey;
        }
      }

      public async ValueTask AddListenerAsync(string? a_RoutingKey, IMessageStreamListener<M> a_Listener)
      {
        string queueName = Topology.ConsumerQueueName(a_RoutingKey);
        if(Topology.ConsumerPersists)
        {
          await Connection.Channel.QueueDeclareAsync(queueName, durable: true, autoDelete: false, exclusive: false);
        }
        else
        {
          await Connection.Channel.QueueDeclareAsync(queueName);
        }
        var rk = RoutingKey(a_RoutingKey);
        await Connection.Channel.QueueBindAsync(queueName, Connection.Topology.ExchangeName, rk);
        var consumer = new AsyncEventingBasicConsumer(Connection.Channel);
        consumer.ReceivedAsync += async (ch, ea)=>
        {
          M msg;
          try
          {
            msg = PBuffer.GetValue<M>(ea.Body);
          }
          catch(Exception e)
          {
            this.Log.LogError(e, "Failed Deserialise");
            return;
          }
          try
          {
            await a_Listener.OnReceivedAsync(msg);
          }
          catch(Exception e)
          {
            this.Log.LogError(e, "Listener borked");
          }
        };
        var consumerTag = await Connection.Channel.BasicConsumeAsync(queueName, true, consumer);
        m_Listeners.TryAdd(a_Listener, consumerTag);
      }

      public async ValueTask DisposeAsync()
      {
        await Connection.DisposeAsync();
      }

      public async ValueTask RemoveListenerAsync(string? a_RoutingKey, IMessageStreamListener<M> a_Listener)
      {
        if(m_Listeners.TryRemove(a_Listener, out var consumerTag))
        {
          await Connection.Channel.BasicCancelAsync(consumerTag);
        }
      }
    }
    class NullStreamFactoryLogger : ILogger<RabbitStreamSourceFactory<M>>
    {
      public IDisposable? BeginScope<TState>(TState state) where TState : notnull=>null;
      public bool IsEnabled(LogLevel logLevel)=>false;
      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
  }
}
