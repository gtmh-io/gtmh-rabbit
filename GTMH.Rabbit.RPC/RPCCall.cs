using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using GTMH.IO;

namespace GTMH.Rabbit.RPC
{
  /// <summary>
  /// Intentionally duplicated in the acronym the Call part of Remote Procedure Call
  /// </summary>
  [ProtoContract]
  public struct RPCCall
  {
    public bool IsDefault => MethodName==null;
    [ProtoMember(20)]
    public string MethodName { get; private set; }
    [ProtoMember(30)]
    public ImmutableArray<ReadOnlyMemory<byte>> Args { get; private set; }
    public RPCCall(string a_MethodName, ImmutableArray<ReadOnlyMemory<byte>> a_Args)
    {
      MethodName =a_MethodName;
      Args = a_Args;
    }
    public string TraceMethodName => string.IsNullOrEmpty(MethodName)?"<Connnect>":MethodName;
    public T Unpack<T>( int a_ArgIndex )
    {
      return ProtoBuf.Serializer.Deserialize<T>(Args[a_ArgIndex]);
    }
    public static RPCCall Pack(string a_MethodName)
    {
      return new RPCCall { MethodName = a_MethodName };
    }
    public static RPCCall Pack<T>(string a_MethodName, T a_Arg0, IMemoryStreamManager ? a_MemManager = null)
    {
      return new RPCCall {  MethodName = a_MethodName, Args = ImmutableArray.Create(Pack(a_Arg0, a_MemManager)) };
    }
    public static RPCCall Pack<T1, T2>(string a_MethodName, T1 a_Arg0, T2 a_Arg1, IMemoryStreamManager ? a_MemManager = null)
    {
      return new RPCCall {  MethodName = a_MethodName, Args = ImmutableArray.Create(Pack(a_Arg0, a_MemManager), Pack(a_Arg1, a_MemManager)) };
    }
    public static RPCCall Pack<T1, T2, T3>(string a_MethodName, T1 a_Arg0, T2 a_Arg1, T3 a_Arg2, IMemoryStreamManager ? a_MemManager = null)
    {
      return new RPCCall {  MethodName = a_MethodName, Args = ImmutableArray.Create(Pack(a_Arg0, a_MemManager), Pack(a_Arg1, a_MemManager), Pack(a_Arg2, a_MemManager)) };
    }
    public static RPCCall Pack<T1, T2, T3, T4>(string a_MethodName, T1 a_Arg0, T2 a_Arg1, T3 a_Arg2, T4 a_Arg3, IMemoryStreamManager ? a_MemManager = null)
    {
      return new RPCCall {  MethodName = a_MethodName, Args = ImmutableArray.Create(Pack(a_Arg0, a_MemManager), Pack(a_Arg1, a_MemManager), Pack(a_Arg2, a_MemManager), Pack(a_Arg3, a_MemManager)) };
    }
    public static RPCCall Pack<T1, T2, T3, T4, T5>(string a_MethodName, T1 a_Arg0, T2 a_Arg1, T3 a_Arg2, T4 a_Arg3, T5 a_Arg4, IMemoryStreamManager ? a_MemManager = null)
    {
      return new RPCCall {  MethodName = a_MethodName, Args = ImmutableArray.Create(Pack(a_Arg0, a_MemManager), Pack(a_Arg1, a_MemManager), Pack(a_Arg2, a_MemManager), Pack(a_Arg3, a_MemManager), Pack(a_Arg4, a_MemManager)) };
    }
    private static ReadOnlyMemory<byte> Pack<T>(T a_Value, IMemoryStreamManager ? a_MemManager)
    {
      using(var stream = a_MemManager.GetStream<T>())
      {
        var impl = stream.Impl();
        ProtoBuf.Serializer.Serialize(impl, a_Value);
        impl.Flush();
        return stream.Content;
      }
    }
  }
}
