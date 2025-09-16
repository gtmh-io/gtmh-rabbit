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
  public abstract class RPCClient : IAsyncDisposable
  {
    public abstract string InterfaceType { get; }
    internal IRPCTopology Topology { get; }

    private readonly IConnectionFactory m_TransportRabbit;
    protected readonly ILogger Log;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<RPCResult>> m_CallsOnFoot = new();
    private IConnection? m_Connection;
    private IChannel? m_PublishChannel;
    private IChannel? m_ConsumeChannel;
    private string ? m_ResponseQueue;
    private string ? QUEUE_NAME;
    public IRPCClientConfig ClientConfig { get; }
    protected readonly string ClientId = Guid.NewGuid().ToString();
    internal readonly string ? MsgTTL;
    public RPCClient(IRPCFactory a_Factory, IRPCTopology a_Topology, ILogger a_Logger, IRPCClientConfig ? a_Config = null)
    {
      this.Topology = a_Topology;
      m_TransportRabbit = a_Factory.Transport.Create();
      Log = a_Logger;
      ClientConfig = a_Config ?? new RPCClientConfig();
      if(ClientConfig.CallTimeout >= 0)
      {
        MsgTTL = (2*ClientConfig.CallTimeout).ToString();
      }
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>::{InterfaceType}[Rabbit={a_Factory.Transport.HostIdentity} Id={ClientId} Timeout={ClientConfig.CallTimeout}]]");
    }

    public async Task Connect()
    {
      QUEUE_NAME = await Topology.FindAsync(this.InterfaceType);
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}][Queue={QUEUE_NAME}]");
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::CreateConnection");
      m_Connection = await m_TransportRabbit.CreateConnectionAsync();
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::CreatePublishChannel");
      m_PublishChannel = await m_Connection.CreateChannelAsync();
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::CreateConsumeChannel");
      m_ConsumeChannel = await m_Connection.CreateChannelAsync();
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::CreateQueue");
      QueueDeclareOk queueDeclareResult = await m_ConsumeChannel.QueueDeclareAsync($"{InterfaceType}::{ClientId}");
      m_ResponseQueue = queueDeclareResult.QueueName;
      var consumer = new AsyncEventingBasicConsumer(m_ConsumeChannel);

      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::Listening[{m_ResponseQueue}]");
      consumer.ReceivedAsync += OnReceivedAsync;

      await m_ConsumeChannel.BasicConsumeAsync(m_ResponseQueue, true, consumer).ConfigureAwait(false);
      if(ClientConfig.ConnectTimeout > 0)
      {
        if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::Pinging...");
        // send a ping to fail quickly if there's no party on the other side
        await DispatchAsync(default(RPCCall)).ConfigureAwait(false);
      }
    }

    private Task OnReceivedAsync(object model, BasicDeliverEventArgs ea)
    {
      var correlationId = ea.BasicProperties.CorrelationId;
      if ( correlationId == null ) return Task.CompletedTask;
      if (m_CallsOnFoot.TryRemove(correlationId, out var tcs))
      {
        if(ea.Body.Length > 0)
        {
          try
          {
            var result = PBuffer.GetValue<RPCResult>(ea.Body);
            tcs.TrySetResult(result);
          }
          catch(Exception e)
          {
            if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::OnReceivedAsync: Error Deserialising{e}");
            tcs.TrySetException(e);
          }
        }
        else
        {
          tcs.SetResult(default(RPCResult));
        }
      }
      return Task.CompletedTask;
    }

    protected async Task<RPCResult> DispatchAsync(RPCCall a_Call)
    {
      if (Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::DispatchAsync::{a_Call.TraceMethodName} ThreadId: {Thread.CurrentThread.ManagedThreadId}");
      if(m_PublishChannel is null || QUEUE_NAME is null ) throw new InvalidOperationException("Not connected");
      // vary the timeout based on call type
      using var cts = new CancellationTokenSource(a_Call.IsDefault ? ClientConfig.ConnectTimeout : ClientConfig.CallTimeout);
      // create this early incase it fails
      var messageBytes = PBuffer.Create(a_Call).Data;

      string correlationId = Guid.NewGuid().ToString();
      var props = new BasicProperties { CorrelationId = correlationId, ReplyTo = m_ResponseQueue, Expiration = MsgTTL }; 
      var tcs = new TaskCompletionSource<RPCResult>( TaskCreationOptions.RunContinuationsAsynchronously);
      m_CallsOnFoot.TryAdd(correlationId, tcs);
      using CancellationTokenRegistration ctr = cts.Token.Register(() => { m_CallsOnFoot.TryRemove(correlationId, out _); tcs.SetCanceled(); });
      bool failure = true;
      try
      {
        await m_PublishChannel.BasicPublishAsync(exchange: string.Empty, routingKey: QUEUE_NAME, mandatory: true, basicProperties: props, body: messageBytes, cts.Token).ConfigureAwait(false);
        failure = false;
      }
      finally
      {
        if(failure)
        {
          m_CallsOnFoot.TryRemove(correlationId, out var _);
        }
      }

      try
      {
        return await tcs.Task.ConfigureAwait(false);
      }
      catch(System.Threading.Tasks.TaskCanceledException)
      {
        throw new RPCTimeout($"Timeout@{ClientConfig.CallTimeout} consider setting --rpc.timeout=longer. Alt you have a re-entrant RPC call and limited concurrency");
      }
    }

    public async ValueTask DisposeAsync()
    {
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::PublishChannelClose");
      if(m_PublishChannel != null)
      {
        try
        {
          using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
          await m_PublishChannel.CloseAsync(cts.Token);
        }
        catch(ObjectDisposedException)
        {
          // swallow silently - these occur when the broker is disappeared whilst client is up
        }
        catch(Exception e)
        {
          Log.LogError(e, $"RPCClient<{InterfaceType}>[{ClientId}]::PublishChannelClose Error");
        }
      }
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::ConsumeChannelClose");
      if(m_ConsumeChannel != null)
      {
        try
        {
          using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
          await m_ConsumeChannel.CloseAsync(cts.Token);
        }
        catch(ObjectDisposedException)
        {
          // swallow silently - these occur when the broker is disappeared whilst client is up
        }
        catch (Exception e)
        {
          Log.LogError(e, $"RPCClient<{InterfaceType}>[{ClientId}]::ConsumeChannelClose Error");
        }
      }
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::ConnectionClose");
      if(m_Connection != null)
      {
        try
        {
          using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
          await m_Connection.CloseAsync(cts.Token);
        }
        catch(ObjectDisposedException)
        {
          // swallow silently - these occur when the broker is disappeared whilst client is up
        }
        catch(Exception e)
        {
          Log.LogError(e, $"RPCClient<{InterfaceType}>[{ClientId}]::ConnectionClose Error");
        }
      }
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::PublishChannelDispose");
      if ( m_PublishChannel!=null) await m_PublishChannel.DisposeAsync();
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::ConsumeChannelDispose");
      if ( m_ConsumeChannel!=null) await m_ConsumeChannel.DisposeAsync();
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::ConnectionDispose");
      if ( m_Connection!=null) await m_Connection.DisposeAsync();
      if ( Log.IsEnabled(LogLevel.Trace)) Log.LogTrace($"RPCClient<{InterfaceType}>[{ClientId}]::Disposed");
    }
  }
}
