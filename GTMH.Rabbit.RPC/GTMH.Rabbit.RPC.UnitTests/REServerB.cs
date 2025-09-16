using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

using GTMH.IDL;

namespace GTMH.Rabbit.RPC.UnitTests
{
  [RPCInterface]
  public interface IREServerB
  {
    [RPCMethod]
    ValueTask<string> BGetInnerValueAsync(char c, int a_Depth);
  }
  class REServerBImplLateConstruction : IREServerB
  {
    private IRPCFactory m_RPC;
    private UTTopology m_Topology;
    private ILogger Log;
    public REServerBImplLateConstruction(IRPCFactory object1, UTTopology topology, ILogger object2)
    {
      this.m_RPC = object1;
      this.m_Topology = topology;
      this.Log = object2;
    }

    public async ValueTask<string> BGetInnerValueAsync(char A, int a_Depth)
    {
      if(a_Depth == 0) return $"{A}B";
      var client = new REServerAClient(m_RPC, m_Topology, Log, new RPCClientConfig { CallTimeout = 4003, ConnectTimeout = 4004 } );
      await client.Connect().ConfigureAwait(false);
      await using(client)
      {
        Log.LogTrace("REServerBImpl::AGetInnerValueAsync....");
        var rval = await client.AGetInnerValueAsync($"{A}B");
        Log.LogTrace("REServerBImpl::GotInnerValueAsync");
        return rval;
      }
    }
  }

  class REServerBImplEarlyConstruction : IREServerB, IAsyncDisposable
  {
    private IRPCFactory m_RPC;
    private UTTopology m_Topology;
    private ILogger Log;
    private REServerAClient ? m_Client;
    public REServerBImplEarlyConstruction(IRPCFactory object1, UTTopology topology, ILogger object2)
    {
      this.m_RPC = object1;
      this.m_Topology = topology;
      this.Log = object2;
    }

    public async ValueTask ConnectA()
    {
      m_Client = new REServerAClient(m_RPC, m_Topology, Log, new RPCClientConfig { CallTimeout = 4003, ConnectTimeout = 4004 } );
      await m_Client.Connect().ConfigureAwait(false);
    }

    public async ValueTask<string> BGetInnerValueAsync(char A, int a_Depth)
    {
      if(a_Depth == 0) return $"{A}B";
      var client = new REServerAClient(m_RPC, m_Topology, Log, new RPCClientConfig { CallTimeout = 4003, ConnectTimeout = 4004 } );
      await client.Connect().ConfigureAwait(false);
      await using(client)
      {
        Log.LogTrace("REServerBImpl::AGetInnerValueAsync....");
        var rval = await client.AGetInnerValueAsync($"{A}B");
        Log.LogTrace("REServerBImpl::GotInnerValueAsync");
        return rval;
      }
    }

    public async ValueTask DisposeAsync()
    {
      if(m_Client != null)
      {
        await m_Client.DisposeAsync();
      }
    }
  }
}
