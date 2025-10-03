using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Text;

using GTMH.IO;

namespace GTMH.Rabbit.RPC
{
  [ProtoContract]
  public struct RPCResult
  {
    [ProtoMember(20)]
    public ReadOnlyMemory<byte> Data { get; private set; }
    [ProtoMember(30)]
    public string ? ExceptionData { get; private set; }

    public T Unpack<T>()
    {
      return ProtoBuf.Serializer.Deserialize<T>(Data);
    }
    public static RPCResult Void() => new RPCResult { };
    public static RPCResult ReturnValue<T>(T a_Value, IMemoryStreamManager ? a_MemManager = null)
    {
      using(var stream = a_MemManager.GetStream<T>())
      {
        var impl = stream.Impl();
        ProtoBuf.Serializer.Serialize(impl, a_Value);
        impl.Flush();
        return new RPCResult { Data = stream.Content };
      }
    }
    public static RPCResult Failure(Exception e) => new RPCResult { ExceptionData = e.ToString() };
  }
}
