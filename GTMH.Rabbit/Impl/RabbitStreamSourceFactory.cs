using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GTMH.Rabbit.Impl
{
  public static class RabbitStreamSourceFactory
  {
    public static RabbitStreamSourceFactory_t<M> Create<M>(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology, ILogger<RabbitStreamSourceFactory_t<M>>? a_Logger = null)
    {
      return new RabbitStreamSourceFactory_t<M>(a_Rabbit, a_Topology, a_Logger);
    }
  }
  public class RabbitStreamSourceFactory_t<M> : IMessageStreamSourceFactory<M> 
  {
    RabbitInstance<M> m_Rabbit;
    private readonly ILogger<RabbitStreamSourceFactory_t<M>> Log;

    public RabbitStreamSourceFactory_t(IRabbitFactory a_Rabbit, IMQTopology<M> a_Topology, ILogger<RabbitStreamSourceFactory_t<M>> ? a_Logger = null)
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


      readonly struct ListenerKey : IEquatable<ListenerKey> 
      {
        private readonly IMessageStreamListener<M> Listener;
        private readonly string? Topic;

        public ListenerKey(IMessageStreamListener<M> a_Listener, string? a_Topic)
        {
          this.Listener = a_Listener;
          this.Topic = a_Topic;

        }
        public bool Equals(ListenerKey other) => this.Listener==other.Listener && this.Topic==other.Topic;
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
          if ( obj == null ) return false;
          return this.Equals((ListenerKey)obj);
        }
        public override int GetHashCode() => (Listener, Topic).GetHashCode();
      }
      class ConsumerTag
      {
        public string Value { get; set; } = "<not_set>";
      }

      ConcurrentDictionary<ListenerKey, ConsumerTag> m_TopicListeners = new();
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
        // we only allow one listener per topic so try add now with a placeholder consumer tag
        // placeholder will do as peeps shouldn't be calling RemoveListener until addlistener's completed
        var lk = new ListenerKey(a_Listener, a_RoutingKey);
        if(!m_TopicListeners.TryAdd(lk, new ConsumerTag()))
        {
          throw new ArgumentException($"Listener with given routing key is already added");
        }
        bool failure = true;
        try
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
          consumer.ReceivedAsync += async (ch, ea) =>
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
          // store the consumer tag
          m_TopicListeners[lk].Value = consumerTag;
          failure = false;
        }
        finally
        {
          if(failure)
          {
            m_TopicListeners.TryRemove(lk, out var _ );
          }
        }
      }

      public async ValueTask DisposeAsync()
      {
        await Connection.DisposeAsync();
      }

      public async ValueTask RemoveListenerAsync(string? a_RoutingKey, IMessageStreamListener<M> a_Listener)
      {
        var lk = new ListenerKey(a_Listener, a_RoutingKey);
        if(m_TopicListeners.TryRemove(lk, out var consumerTag))
        {
          await Connection.Channel.BasicCancelAsync(consumerTag.Value);
        }
      }
    }
    class NullStreamFactoryLogger : ILogger<RabbitStreamSourceFactory_t<M>>
    {
      public IDisposable? BeginScope<TState>(TState state) where TState : notnull=>null;
      public bool IsEnabled(LogLevel logLevel)=>false;
      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
  }
}
