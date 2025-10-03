using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

using GTMH.IDL;

namespace GTMH.Rabbit.RPC.UnitTests
{
  [RPCInterface]
  public interface IREServerA
  {
    [RPCMethod]
    ValueTask<string> GetOuterValueAsync(char A, int a_Depth);
    [RPCMethod]
    ValueTask<string> AGetInnerValueAsync(string a_Str);
  }
  class REServerAImpl : IREServerA
  {
    private IRPCFactory m_RPC;
    private UTTopology m_Topology;
    private ILogger Log;
    public REServerAImpl(IRPCFactory object1, UTTopology topology, ILogger object2)
    {
      this.m_RPC = object1;
      this.m_Topology = topology;
      this.Log = object2;
    }

    public ManualResetEvent InnerMethodIsCalled = new ManualResetEvent(false);
    public ValueTask<string> AGetInnerValueAsync(string a_Str)
    {
      InnerMethodIsCalled.Set();
      Log.LogTrace("REServerAImpl::AGetInnerValueAsync....");
      var rval = ValueTask.FromResult($"{a_Str}C");
      Log.LogTrace("REServerAImpl::GotInnerValueAsync");
      return rval;
    }

    public async ValueTask<string> GetOuterValueAsync(char A, int a_Depth)
    {
      if ( a_Depth == 0 ) return $"{A}";
      var client = new REServerBClient(m_RPC, m_Topology, Log, new RPCClientConfig { CallTimeout = 4001, ConnectTimeout = 4002 } );
      await client.Connect().ConfigureAwait(false);
      await using(client)
      {
        Log.LogTrace("REServerAImpl::GetOuterValueAsync....");
        var rval = await client.BGetInnerValueAsync(A, --a_Depth).ConfigureAwait(false);
        Log.LogTrace("REServerAImpl::GotOuterValueAsync");
        return rval;
      }
    }
  }
}
