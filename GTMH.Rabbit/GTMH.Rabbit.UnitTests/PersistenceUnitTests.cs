using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;
using Moq;
using GTMH.Rabbit.Impl;

// https://tunit.dev/

namespace GTMH.Rabbit.UnitTests
{
  public class PersistenceUnitTests : MQUnitTests
  {
    class UTTransientQueueTopology<M> : TransientQueueTopology_t<M>
    {
      readonly string Scope = Guid.NewGuid().ToString();
      public override string ExchangeName => $"{Scope}::{base.ExchangeName}";
      public override string ConsumerQueueName(string? a_RoutingKey) => $"{Scope}::{base.ConsumerQueueName(a_RoutingKey)}";
    }
    [Test]
    public async ValueTask TestPersistenceNotPersistent()
    {
      var topology = new UTTransientQueueTopology<Msg>();
      var transientFact = new RabbitStreamSourceFactory<Msg>(RF, topology, MsgSrcLog.Object);
      var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
      var l = new Listener<Msg>();
      var sink = await sinkFact.CreateSink();
      await using(sink)
      {
        {
          var src = await transientFact.CreateSource();
          await using(src)
          {
            await src.AddListenerAsync(null, l);
            await sink.PublishAsync("a", Msg.New());
            await Task.Delay(WaitRabbitDispatch);
            await Assert.That(l.Recvd.Count).IsEqualTo(1);
          }
        }
        await sink.PublishAsync("a", Msg.New());
        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(l.Recvd.Count).IsEqualTo(1);
        {
          var src = await transientFact.CreateSource();
          await using(src)
          {
            await src.AddListenerAsync(null, l);
            await Task.Delay(WaitRabbitDispatch);
            await Assert.That(l.Recvd.Count).IsEqualTo(1); // msg is dropped
          }
        }
      }
    }
    class UTPersistentQueueTopology<M> : PersistentQueueTopology<M>
    {
      readonly string Scope = Guid.NewGuid().ToString();
      public UTPersistentQueueTopology() : base(Guid.NewGuid().ToString()) { }
      public override string ExchangeName => $"{Scope}::{base.ExchangeName}";
      public override string ConsumerQueueName(string? a_RoutingKey) => $"{Scope}::{base.ConsumerQueueName(a_RoutingKey)}";
    }
    [Test]
    public async ValueTask TestPersistenceIs()
    {
      var topology = new UTPersistentQueueTopology<Msg>();
      var srcFact = new RabbitStreamSourceFactory<Msg>(RF, topology, MsgSrcLog.Object);
      var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
      var l = new Listener<Msg>();
      var sink = await sinkFact.CreateSink();
      await using(sink)
      {
        {
          var src = await srcFact.CreateSource();
          await using(src)
          {
            await src.AddListenerAsync(null, l);
            await sink.PublishAsync("a", Msg.New());
            await Task.Delay(WaitRabbitDispatch);
            await Assert.That(l.Recvd.Count).IsEqualTo(1);
          }
        }
        await sink.PublishAsync("a", Msg.New());
        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(l.Recvd.Count).IsEqualTo(1);
        {
          var src = await srcFact.CreateSource();
          await using(src)
          {
            await src.AddListenerAsync(null, l);
            await Task.Delay(WaitRabbitDispatch);
            await Assert.That(l.Recvd.Count).IsEqualTo(2); // msg not dropped
          }
        }
      }
    }
    [Test]
    public async ValueTask TestPersistenceSubOptimal()
    {
      var topology = new UTPersistentQueueTopology<Msg>();
      var srcFact = new RabbitStreamSourceFactory<Msg>(RF, topology, MsgSrcLog.Object);
      var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
      var l1 = new Listener<Msg>();
      var l2 = new Listener<Msg>();
      var sink = await sinkFact.CreateSink();
      await using(sink)
      {
        var src = await srcFact.CreateSource();
        await using(src)
        {
          await src.AddListenerAsync(null, l1);
          await src.AddListenerAsync(null, l2);
          await sink.PublishAsync("a", Msg.New());
          await Task.Delay(WaitRabbitDispatch);
          await Assert.That(l1.Recvd.Count+l2.Recvd.Count).IsEqualTo(1); // not 2
        }
      }
    }
  }
}
