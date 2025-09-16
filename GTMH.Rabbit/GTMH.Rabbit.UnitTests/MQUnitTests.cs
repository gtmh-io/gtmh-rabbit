using Microsoft.Extensions.Logging;

using Moq;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using GTMH.Rabbit.Impl;
using GTMH.Security;

namespace GTMH.Rabbit.UnitTests
{
  public class MQUnitTests
  {
    [ProtoContract(SkipConstructor=true)]
    [DebuggerStepThrough]
    public class Msg
    {
      [ProtoMember(1)]
      public string Content { get; }
      public Msg(string a_Content) { this.Content=a_Content; }
      public static Msg New()=>new Msg(Guid.NewGuid().ToString());
      public static Msg New(string a_Content)=>new Msg(a_Content);
    }
    public const int WaitRabbitDispatch = 1000;
    public readonly string Scope = Guid.NewGuid().ToString();
    public readonly RabbitFactory RF = new RabbitFactory(new RabbitConfig { Host = "localhost" }, new PlainText());
    public readonly Mock<ILogger<RabbitStreamSourceFactory<Msg>>> MsgSrcLog = new();
    public readonly Mock<ILogger<RabbitStreamSinkFactory<Msg>>> MsgSinkLog = new();
  }
}
