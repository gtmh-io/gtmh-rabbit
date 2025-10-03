using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GTMH.Rabbit.RPC
{
  public abstract class RPCServer : IRPCServer
  {
    private readonly IConnectionFactory m_Rabbit;
    internal readonly IRPCTopology Topology;
    internal readonly ILogger Log;

    public abstract string InterfaceType { get; }
    internal readonly string QUEUE_NAME;
    private readonly int QueueTTL;
    private readonly ushort MaxConcurrency;

    public RPCServer(IRPCFactory a_RPCEnv, IRPCTopology ? a_Topology, ILogger ? a_Log)
    {
      this.m_Rabbit=a_RPCEnv.Transport.Create();
      this.QueueTTL = a_RPCEnv.ServerQueueTTL;
      this.MaxConcurrency = a_RPCEnv.ServerMaxConcurrency;
      this.Topology = a_Topology ?? new BasicTopology();
      this.Log=a_Log??new NullLogger();
      QUEUE_NAME=Topology.QueueName(InterfaceType);
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>[Queue={QUEUE_NAME} Rabbit={a_RPCEnv.Transport.HostIdentity} TTL={QueueTTL} MaxConcurrency={MaxConcurrency}]");
    }

    public async ValueTask<IAsyncDisposable> Publish()
    {
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::CreateConnnection");
      var connection = await m_Rabbit.CreateConnectionAsync();
      IChannel ? receiveChannel = null;
      IChannel ? sendChannel = null;
      AsyncEventingBasicConsumer ? consumer = null;
      DispatchListener ? dispatchListener = null;
      Publication ? rval = null;
      try
      {
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::CreateRecvChannel");
        receiveChannel = await connection.CreateChannelAsync();

        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::CreateSendChannel");
        sendChannel = await connection.CreateChannelAsync();

        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::DeclareQueue");
        var arguments = new Dictionary<string, object?>();
        if(QueueTTL > 0)
        {
          arguments["x-message-ttl"] = QueueTTL;
        }
        await receiveChannel.QueueDeclareAsync(queue: QUEUE_NAME, durable: false, exclusive: false, autoDelete: true, arguments: arguments);
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::QOS");
        await receiveChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: MaxConcurrency, global: false);

        consumer = new AsyncEventingBasicConsumer(receiveChannel);
        dispatchListener = new DispatchListener(this, sendChannel);
        if ( Log.IsEnabled(LogLevel.Trace) ) Log.LogTrace($"RPCServer<{InterfaceType}>::BasicConsume");
        await receiveChannel.BasicConsumeAsync(QUEUE_NAME, false, consumer);

        if ( Log.IsEnabled(LogLevel.Trace) ) Log.LogTrace($"RPCServer<{InterfaceType}>::AddTopology");
        await Topology.AddAsync(this);

        // finally start to consume messages
        consumer.ReceivedAsync += dispatchListener.Consumer_ReceivedAsync;

        rval = new Publication(this, dispatchListener, connection, receiveChannel, sendChannel, ()=>consumer.ReceivedAsync -= dispatchListener.Consumer_ReceivedAsync);
        return rval;
      }
      finally
      {
        if(rval == null)
        {
          // avoid adding noise to the broken signal - don't log stuff here 
          try { await Topology.RemoveAsync(this); } catch { }
          if(consumer != null && dispatchListener != null) try { consumer.ReceivedAsync -= dispatchListener.Consumer_ReceivedAsync; } catch { }
          if(receiveChannel != null)
          {
            try
            {
              using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
              await receiveChannel.AbortAsync(cts.Token);
            } catch { }
          }
          if(sendChannel != null)
          {
            try
            {
              using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
              await sendChannel.AbortAsync(cts.Token);
            }
            catch { }
          }
          if(connection != null)
          {
            try
            {
              using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
              await connection.CloseAsync(cts.Token); // TODO no AbortAsync on IConnection
            }
            catch { }
          }

          if ( receiveChannel != null ) await receiveChannel.DisposeAsync();
          if ( sendChannel != null ) await sendChannel.DisposeAsync();
          if ( connection != null ) await connection.DisposeAsync();
        }
      }
    }
    public abstract ValueTask<RPCResult> Dispatch(RPCCall a_Call);

    class DispatchListener : IAsyncDisposable
    {
      private ILogger Log => m_Server.Log;
      private string InterfaceType => m_Server.InterfaceType;
      private readonly IChannel sendChannel;
      private readonly RPCServer m_Server;
      private ConcurrentDictionary<Task, byte> m_TasksOnFoot = new();

      public DispatchListener(RPCServer a_Server, IChannel sendChannel)
      {
        this.m_Server= a_Server;;
        this.sendChannel = sendChannel;
      }

      public Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs ea)
      {
        // Don't await the processing - let it run concurrently
        var processingTask = ProcessMessageAsync(sender, ea);
        m_TasksOnFoot.TryAdd(processingTask, 0);
        _ = processingTask.ContinueWith( t=>
        {
          m_TasksOnFoot.TryRemove(t, out _ );
        }, TaskContinuationOptions.ExecuteSynchronously);
        // return a completed task so we don't block
        return Task.CompletedTask;
      }

      public async Task ProcessMessageAsync(object sender, BasicDeliverEventArgs ea)
      {
        AsyncEventingBasicConsumer cons = (AsyncEventingBasicConsumer)sender;
        IChannel receiveChannel = cons.Channel;

        IReadOnlyBasicProperties props = ea.BasicProperties;
        var replyProps = new BasicProperties { CorrelationId = props.CorrelationId };

        var call = PBuffer.GetValue<RPCCall>(ea.Body);
        if(call.IsDefault)
        {
          if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Dispatch::Respond::Connect");
          await sendChannel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!, mandatory: true, basicProperties: replyProps, body: default).ConfigureAwait(false);
          if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Dispatch::Ack::Connect");
          await receiveChannel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false).ConfigureAwait(false);
        }
        else
        {
          RPCResult result;
          try
          {
            if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Dispatch::{call.MethodName}");
            result = await m_Server.Dispatch(call).ConfigureAwait(false);
          }
          catch(Exception e)
          {
            if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Dispatch::Error::{call.MethodName} {e}");
            result = RPCResult.Failure(e);
          }
          var pb = PBuffer.Create(result);
          if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Dispatch::Respond::{call.MethodName}");
          await sendChannel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!, mandatory: true, basicProperties: replyProps, body: pb.Data) .ConfigureAwait(false);
          if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Ack::{call.MethodName}");
          await receiveChannel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false).ConfigureAwait(false);
        }
      }

      public async ValueTask DisposeAsync()
      {
        var activeTasks = m_TasksOnFoot.Keys.ToArray();
        if(activeTasks.Any())
        {
          if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Dispatch::WaitCompletion[OnFoot={activeTasks.Length}");
          try
          {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Task.WhenAll(activeTasks).WaitAsync(cts.Token);
          }
          catch(OperationCanceledException)
          {
            if(Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCServer<{InterfaceType}>::Dispatch::WaitCompletionTimedOut");
          }
        }
      }
    }

    class Publication : IAsyncDisposable
    {
      private readonly DispatchListener Dispatcher;
      private readonly RPCServer Server;
      private readonly IConnection m_RabbitConnection;
      private readonly IChannel RecvChannel;
      private readonly IChannel SendChannel;
      private readonly Action ConsumerUnsubscribe;

      public Publication(RPCServer a_Server, DispatchListener dispatchListener, IConnection a_RabbitConnection, IChannel recvChannel, IChannel sendChannel, Action a_ConsumerUnsubscribe)
      {
        this.Dispatcher = dispatchListener;
        this.Server = a_Server;
        this.m_RabbitConnection = a_RabbitConnection;
        this.RecvChannel = recvChannel;
        this.SendChannel = sendChannel;
        this.ConsumerUnsubscribe = a_ConsumerUnsubscribe;
      }

      public async ValueTask DisposeAsync()
      {
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::RemoveTopology");
        try
        {
          await Server.Topology.RemoveAsync(Server);
        }
        catch (Exception e)
        {
          Server.Log.LogError(e,$"RPCServer::{Server.InterfaceType}>::RemoveTopology Error");
        }
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::Unsubscribe");
        try
        {
          this.ConsumerUnsubscribe();
        }
        catch (Exception e)
        {
          Server.Log.LogError(e,$"RPCServer::{Server.InterfaceType}>::Unsubscribe Error");
        }
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::DisposeDispatcher");
        await Dispatcher.DisposeAsync();

        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::CloseRecvChannel");
        try
        {
          using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
          await RecvChannel.CloseAsync(cts.Token);
        }
        catch (Exception e)
        {
          Server.Log.LogError(e,$"RPCServer::{Server.InterfaceType}>::CloseRecvChannel Error");
        }
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::CloseSendChannel");
        try
        {
          using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
          await SendChannel.CloseAsync(cts.Token);
        }
        catch (Exception e)
        {
          Server.Log.LogError(e,$"RPCServer::{Server.InterfaceType}>::CloseSendChannel Error");
        }
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::CloseRabbit");
        try
        {
          using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
          await m_RabbitConnection.CloseAsync(cts.Token);
        }
        catch (Exception e)
        {
          Server.Log.LogError(e,$"RPCServer::{Server.InterfaceType}>::CloseRabbit Error");
        }
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::DisposeRecvChannel");
        await RecvChannel.DisposeAsync();
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::DisposeSendChannel");
        await SendChannel.DisposeAsync();
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::DisposeRabbit");
        await m_RabbitConnection.DisposeAsync();
        if ( Server.Log.IsEnabled(LogLevel.Trace) ) Server.Log.LogTrace($"RPCServer::{Server.InterfaceType}>::Down");
      }
    }
  }
}
