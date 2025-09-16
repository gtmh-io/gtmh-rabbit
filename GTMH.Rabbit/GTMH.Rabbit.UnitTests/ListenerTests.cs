using Microsoft.Extensions.Logging;

using Moq;

using GTMH.Rabbit.Impl;
namespace GTMH.Rabbit.UnitTests;

// https://tunit.dev/

public class ListenerTests : MQUnitTests
{
  [Test]
  public async ValueTask TestSubscriptions()
  {
    var topology = new UTDefaultTopology<Msg>();
    var srcFact = new RabbitStreamSourceFactory<Msg>(RF, topology, MsgSrcLog.Object);
    var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
    var listener_a1 = new Listener<Msg>();
    var listener_a2 = new Listener<Msg>();
    var listener_b = new Listener<Msg>();
    var listener_wild = new Listener<Msg>();
    var src = await srcFact.CreateSource();
    await using(src)
    {
      await src.AddListenerAsync("a", listener_a1);
      await src.AddListenerAsync("a", listener_a2);
      await src.AddListenerAsync("b", listener_b);
      await src.AddListenerAsync("*", listener_wild);
      var sink = await sinkFact.CreateSink();
      await using(sink)
      {
        await sink.PublishAsync("a", Msg.New());  
        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(listener_a1.Recvd.Count).IsEqualTo(1);
        await Assert.That(listener_a2.Recvd.Count).IsEqualTo(1);
        await Assert.That(listener_b.Recvd.Count).IsEqualTo(0);
        await Assert.That(listener_wild.Recvd.Count).IsEqualTo(1);
        await src.RemoveListenerAsync("a", listener_a2);
        await sink.PublishAsync("a", Msg.New());  

        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(listener_a1.Recvd.Count).IsEqualTo(2);
        await Assert.That(listener_a2.Recvd.Count).IsEqualTo(1);
        await Assert.That(listener_b.Recvd.Count).IsEqualTo(0);
        await Assert.That(listener_wild.Recvd.Count).IsEqualTo(2);
      }
    }
  }
  [Test]
  public async ValueTask TestPublishTopicEmptyThrows()
  {
    var topology = new UTDefaultTopology<Msg>();
    var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
    var sink = await sinkFact.CreateSink();
    await using(sink)
    {
      await Assert.ThrowsAsync<ArgumentException>(async ()=>await sink.PublishAsync("", Msg.New()));
    }
  }
  [Test]
  public async ValueTask TestSubscriptionEmptyNull()
  {
    var topology = new UTDefaultTopology<Msg>();
    var srcFact = new RabbitStreamSourceFactory<Msg>(RF, topology, MsgSrcLog.Object);
    var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
    var l_wild_empty = new Listener<Msg>();
    var l_a = new Listener<Msg>();
    var l_wild_explicit = new Listener<Msg>();
    var l_wild_implicit = new Listener<Msg>();
    var src = await srcFact.CreateSource();
    await using(src)
    {
      await src.AddListenerAsync("", l_wild_empty);
      await src.AddListenerAsync("a", l_a);
      await src.AddListenerAsync("*", l_wild_explicit);
      await src.AddListenerAsync(null, l_wild_implicit);
      var sink = await sinkFact.CreateSink();
      await using(sink)
      {
        await sink.PublishAsync("b", Msg.New());
        await Task.Delay(WaitRabbitDispatch);
        
        await Assert.That(l_wild_empty.Recvd.Count).IsEqualTo(1);
        await Assert.That(l_a.Recvd.Count).IsEqualTo(0);
        // wild does not get empty routing key
        await Assert.That(l_wild_explicit.Recvd.Count).IsEqualTo(1);
        await Assert.That(l_wild_implicit.Recvd.Count).IsEqualTo(1);
      }
    }
  }
  [Test]
  public async ValueTask TestSubscriptionNullIsWild()
  {
    var topology = new UTDefaultTopology<Msg>();
    var srcFact = new RabbitStreamSourceFactory<Msg>(RF, topology, MsgSrcLog.Object);
    var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
    var l_null = new Listener<Msg>();
    var l_empty = new Listener<Msg>();
    var src = await srcFact.CreateSource();
    await using(src)
    {
      await src.AddListenerAsync(null, l_null);
      await src.AddListenerAsync("", l_empty);
      var sink = await sinkFact.CreateSink();
      await using(sink)
      {
        await sink.PublishAsync("a", Msg.New());
        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(l_null.Recvd.Count).IsEqualTo(1);
        await Assert.That(l_empty.Recvd.Count).IsEqualTo(1);
      }
    }
  }
  [Test]
  public async ValueTask TestListenerThrows()
  {
    var topology = new UTDefaultTopology<Msg>();
    var srcFact = new RabbitStreamSourceFactory<Msg>(RF, topology, MsgSrcLog.Object);
    var sinkFact = new RabbitStreamSinkFactory<Msg>(RF, topology, MsgSinkLog.Object);
    var l = new Listener<Msg>();
    var src = await srcFact.CreateSource();
    await using(src)
    {
      await src.AddListenerAsync(null, l);
      var sink = await sinkFact.CreateSink();
      await using(sink)
      {
        await sink.PublishAsync("a", Msg.New());
        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(l.Recvd.Count).IsEqualTo(1);
        l.Throw.Set();
        await sink.PublishAsync("a", Msg.New());
        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(l.Recvd.Count).IsEqualTo(1);
        l.Throw.Reset();
        await sink.PublishAsync("a", Msg.New());
        await Task.Delay(WaitRabbitDispatch);
        await Assert.That(l.Recvd.Count).IsEqualTo(2);
      }
    }
  }
}
