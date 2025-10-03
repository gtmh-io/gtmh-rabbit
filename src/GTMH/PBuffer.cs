using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Text;

using GTMH.IO;

namespace GTMH
{
  [ProtoContract(SkipConstructor=true)]
  public class PBuffer
  {
    [ProtoMember(10)]
    private ReadOnlyMemory<byte> m_Data;
    public ReadOnlyMemory<byte> Data { get { return m_Data; } }
    public PBuffer(ReadOnlyMemory<byte> data) { m_Data = data; }
    public static PBuffer Create<T>(T a_Value, IMemoryStreamManager ? a_MemManager = null)
    {
      using(var stream = a_MemManager.GetStream<T>())
      {
        var impl = stream.Impl();
        ProtoBuf.Serializer.Serialize(impl, a_Value);
        impl.Flush();
        return new PBuffer(stream.Content);
      }
    }
    public T GetValue<T>()
    {
      return ProtoBuf.Serializer.Deserialize<T>(m_Data);
    }
    public static T GetValue<T>(ReadOnlyMemory<byte> a_Data)=> ProtoBuf.Serializer.Deserialize<T>(a_Data);

    public static T Clone<T>(T a_Value, IMemoryStreamManager? a_MemManager = null)
    {
      return Create(a_Value, a_MemManager).GetValue<T>();
    }
  }
}
