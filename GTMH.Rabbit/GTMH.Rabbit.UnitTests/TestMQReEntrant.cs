using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.Extensions.Logging;
using Moq;
using ProtoBuf;

using Tofye.IMQ.Impl;

namespace Tofye.IMQ.UnitTests;

public class TestMQReEntrant : MQUnitTests
{
  public class ListenerL : IMessageStreamListener<ListenerL.Msg>, IMessageStreamListener<MsgR>, IAsyncDisposable
  {
    [ProtoContract(SkipConstructor=true)]
    [DebuggerStepThrough]
    public class Msg
    {
      [ProtoMember(1)]
      public string Content { get; }
      public Msg(string a_Content) { this.Content=a_Content; }
      public static Msg New(string a_Content)=>new Msg(a_Content);
    }
    public ManualResetEvent m_Subscribed = new ManualResetEvent(false);
    public ManualResetEvent m_ReceivedR = new ManualResetEvent(false);
    private RabbitStreamSourceFactory<MsgR> srcFact_r;
    private RabbitStreamSinkFactory<MsgR> sinkFact_r;
    private IMessageStreamSource<MsgR> ? src_r;
    private IMessageStreamSink<MsgR> ? sink_r;

    public ListenerL(RabbitFactory RF, IMQTopology<MsgR> topology_r)
    {
      Mock<ILogger<RabbitStreamSourceFactory<MsgR>>> MsgSrcLog_r = new();
      Mock<ILogger<RabbitStreamSinkFactory<MsgR>>> MsgSinkLog_r = new();
      this.srcFact_r = new RabbitStreamSourceFactory<MsgR>(RF, topology_r, MsgSrcLog_r.Object);
      this.sinkFact_r = new RabbitStreamSinkFactory<MsgR>(RF, topology_r, MsgSinkLog_r.Object);
    }

    public bool Subscribed => m_Subscribed.WaitOne(0);
    public bool ReceivedR => m_ReceivedR.WaitOne(0);
    public List<Msg> Msgs = new();
    public async ValueTask OnReceivedAsync(Msg a_Msg)
    {
      lock(Msgs) Msgs.Add(a_Msg);
      switch(a_Msg.Content)
      {
        case "subscribe":
        {
          if(src_r == null)
          {
            this.src_r = await srcFact_r.CreateSource();
          }
          await src_r.AddListenerAsync("*", this);
          m_Subscribed.Set();
          break;
        }
        case "publish":
        {
          this.sink_r = await sinkFact_r.CreateSink();
          await sink_r.PublishAsync("*", MsgR.New("published"));
          break;
        }
        case "unsubscribe":
        {
          if(src_r != null)
          {
            await src_r.RemoveListenerAsync("*", this);
            m_ReceivedR.Reset();
          }
          break;
        }
      }
    }
    public List<MsgR> MsgRs = new();

    public ValueTask OnReceivedAsync(MsgR a_Msg)
    {
      lock(MsgRs) MsgRs.Add(a_Msg);

      m_ReceivedR.Set();
      return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
      if(src_r != null)
      {
        await src_r.DisposeAsync();
      }
      if(sink_r != null)
      {
        await sink_r.DisposeAsync();
      }
    }
  }
  [ProtoContract(SkipConstructor=true)]
  [DebuggerStepThrough]
  public class MsgR
  {
    [ProtoMember(1)]
    public string Content { get; }
    public MsgR(string a_Content) { this.Content=a_Content; }
    public static MsgR New(string a_Content)=>new MsgR(a_Content);
  }
  [Test]
  public async ValueTask TestRentrancy()
  {
    Mock<ILogger<RabbitStreamSourceFactory<ListenerL.Msg>>> MsgSrcLog_l = new();
    Mock<ILogger<RabbitStreamSinkFactory<ListenerL.Msg>>> MsgSinkLog_l = new();
    Mock<ILogger<RabbitStreamSinkFactory<MsgR>>> MsgSinkLog_r = new();
    var topology_l = new UTDefaultTopology<ListenerL.Msg>();
    var topology_r = new UTDefaultTopology<MsgR>();
    var srcFact_l = new RabbitStreamSourceFactory<ListenerL.Msg>(RF, topology_l, MsgSrcLog_l.Object);
    var sinkFact_l = new RabbitStreamSinkFactory<ListenerL.Msg>(RF, topology_l, MsgSinkLog_l.Object);
    var sinkFact_r = new RabbitStreamSinkFactory<MsgR>(RF, topology_r, MsgSinkLog_r.Object);
    var src_l = await srcFact_l.CreateSource();
    await using(src_l)
    {
      var ll = new ListenerL(RF, topology_r);
      await using(ll)
      {
        await src_l.AddListenerAsync(null, ll);
        var sink_l = await sinkFact_l.CreateSink();
        var sink_r = await sinkFact_r.CreateSink();
        await using(sink_l)
        await using(sink_r)
        {
          await sink_l.PublishAsync("*", ListenerL.Msg.New("subscribe"));
          await Task.Delay(WaitRabbitDispatch);
          await Assert.That(ll.Subscribed).IsTrue();
          await sink_r.PublishAsync("*", MsgR.New("respond"));
          await Task.Delay(WaitRabbitDispatch);
          await Assert.That(ll.ReceivedR).IsTrue();
          await sink_l.PublishAsync("*", ListenerL.Msg.New("publish"));
          await Task.Delay(WaitRabbitDispatch);
          await Assert.That(ll.MsgRs.Select(_=>_.Content)).IsEquivalentTo(new[] { "respond", "published" } );
          await sink_l.PublishAsync("*", ListenerL.Msg.New("unsubscribe"));
          await Task.Delay(WaitRabbitDispatch);
          await Assert.That(ll.ReceivedR).IsFalse();
          await sink_r.PublishAsync("*", MsgR.New("respond"));
          await Assert.That(ll.ReceivedR).IsFalse();
          await Task.Delay(WaitRabbitDispatch);
          await Assert.That(ll.MsgRs.Select(_=>_.Content)).IsEquivalentTo(new[] { "respond", "published" } );
        }
      }
    }
  }
}
