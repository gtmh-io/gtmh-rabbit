using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace GTMH.Rabbit.Impl
{
  public class RabbitInstance<M>
  {
    private readonly IConnectionFactory Rabbit;
    protected readonly ILogger Log;

    public readonly IMQTopology<M> Topology;
    private readonly string Context;

    public RabbitInstance(IRabbitFactory a_RabbitFactory, IMQTopology<M> a_Topology, ILogger a_Log)
    {
      this.Topology=a_Topology;
      Context=$"RabbitStream<{Topology.ExchangeName}>";
      this.Rabbit = a_RabbitFactory.Create();
      this.Log = a_Log;
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}[Rabbit={a_RabbitFactory.HostIdentity}]");
    }

    public async ValueTask<StreamConnection<M>> Connect(CancellationToken a_Cancel = default)
    {
      StreamConnection<M> ? rval = null;
      IConnection ? connection = null;
      IChannel ? channel = null;
      try
      {
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::CreateConnection");
        connection = await this.Rabbit.CreateConnectionAsync();
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::CreateChannel");
        channel = await connection.CreateChannelAsync();
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::ExchangeDeclare");
        await channel.ExchangeDeclareAsync(Topology.ExchangeName, "topic", durable: true, autoDelete: false);
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::Connected");
        rval = new StreamConnection<M>(Topology, Log, Context, connection, channel);
        return rval;
      }
      finally
      {
        if(rval == null)
        {
          if(channel != null) try { await channel.CloseAsync(); } catch { }
          if ( connection != null ) try { await connection.CloseAsync(); } catch { }

          if ( channel != null ) await channel.DisposeAsync();
          if (connection != null ) await connection.DisposeAsync();
        }
      }
    }

    public class StreamConnection<T> : IAsyncDisposable
    {
      private readonly IConnection connection;
      public readonly IChannel Channel;
      private readonly string Context;
      public readonly ILogger Log;
      public readonly IMQTopology<M> Topology;

      public StreamConnection(IMQTopology<M> Topology, ILogger log, string context, IConnection connection, IChannel channel)
      {
        this.Topology = Topology;
        this.Context = context;
        this.Log = log;
        this.connection = connection;
        this.Channel = channel;
      }

      public async ValueTask DisposeAsync()
      {
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::CloseChannel");
        try { await Channel.CloseAsync(); } catch { }
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::CloseConnection");
        try { await connection.CloseAsync(); } catch { }
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::DisposeChannel");
        await Channel.DisposeAsync();
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"{Context}::DisposeConnection");
        await connection.DisposeAsync();
      }
    }
  }
}
