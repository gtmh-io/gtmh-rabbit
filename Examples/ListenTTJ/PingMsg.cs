using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Tofye.Infrastructure
{
  [DataContract][ProtoContract(SkipConstructor=true)]
  public class PingMsg
  {
    private string m_UserDefined;
    [DataMember(Order = 10)][ProtoMember(10)]
    public string UserDefined { get { return m_UserDefined; } private set { m_UserDefined = value; } }

    private string m_Target;
    [DataMember(Order = 20)][ProtoMember(20)]
    public string Target { get { return m_Target; } internal set { m_Target = value; } }

    private DateTime m_ResponseTime;
    [DataMember(Order = 40)][ProtoMember(40)]
    public DateTime ResponseTime { get { return m_ResponseTime; } internal set { m_ResponseTime = value; } }

    public bool IsRequest { get { return ResponseTime == DateTime.MinValue; } }

    public PingMsg(string a_UserDefined, string a_TargetHost)
    {
      m_ResponseTime = DateTime.MinValue;
      m_UserDefined = a_UserDefined ?? "";
      m_Target = a_TargetHost;
    }

    public PingMsg(PingMsg a_Request)
    {
      m_ResponseTime = DateTime.UtcNow;
      m_UserDefined = a_Request.UserDefined;
      m_Target = a_Request.Target;
    }
  }
}
