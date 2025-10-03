using Microsoft.Extensions.Logging;

using Moq;

using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using GTMH.IDL;

namespace GTMH.Rabbit.RPC.UnitTests
{
  public class TestReEntrant : RPCUnitTests
  {
    [Test]
    public async ValueTask TestReEntrantServersA()
    {
      var topology = new UTTopology();
      var serverAImpl = new REServerAImpl(RPCFactory.Object, topology, Logger);
      var dispatchA = new REServerAServiceHost(RPCFactory.Object, topology, Logger, serverAImpl);
      var serverA = await dispatchA.Publish();
      await using(serverA)
      {
        var serverBImpl = new REServerBImplLateConstruction(RPCFactory.Object, topology, Logger);
        var dispatchB = new REServerBServiceHost(RPCFactory.Object, topology, Logger, serverBImpl);
        var serverB = await dispatchB.Publish();
        await using(serverB)
        {
          var client = new REServerAClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 4005, ConnectTimeout = 4006 } );
          await client.Connect();
          await using(client)
          {
            var value = await client.GetOuterValueAsync('A', 0);
            await Assert.That(value).IsEqualTo("A");
          }
        }
      }
    }
    [Test]
    public async ValueTask TestReEntrantServersAB()
    {
      var topology = new UTTopology();
      var serverAImpl = new REServerAImpl(RPCFactory.Object, topology, Logger);
      var dispatchA = new REServerAServiceHost(RPCFactory.Object, topology, Logger, serverAImpl);
      var serverA = await dispatchA.Publish();
      await using(serverA)
      {
        var serverBImpl = new REServerBImplLateConstruction(RPCFactory.Object, topology, Logger);
        var dispatchB = new REServerBServiceHost(RPCFactory.Object, topology, Logger, serverBImpl);
        var serverB = await dispatchB.Publish();
        await using(serverB)
        {
          var client = new REServerAClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 4007, ConnectTimeout = 4008 } );
          await client.Connect();
          await using(client)
          {
            var value = await client.GetOuterValueAsync('A', 1);
            await Assert.That(value).IsEqualTo("AB");
          }
        }
      }
    }
    [Test]
    public async ValueTask TestReEntrantServersABCLateConstruction()
    {
      var topology = new UTTopology();
      var serverAImpl = new REServerAImpl(RPCFactory.Object, topology, Logger);
      var dispatchA = new REServerAServiceHost(RPCFactory.Object, topology, Logger, serverAImpl);
      var serverA = await dispatchA.Publish();
      await using(serverA)
      {
        var serverBImpl = new REServerBImplLateConstruction(RPCFactory.Object, topology, Logger);
        var dispatchB = new REServerBServiceHost(RPCFactory.Object, topology, Logger, serverBImpl);
        var serverB = await dispatchB.Publish();
        await using(serverB)
        {
          var client = new REServerAClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 4009, ConnectTimeout = 4010 } );
          await client.Connect();
          await using(client)
          {
            var value = await client.GetOuterValueAsync('A', 2);
            await Assert.That(value).IsEqualTo("ABC");
          }
        }
      }
    }
    [Test]
    public async ValueTask TestReEntrantServersABCEarlyConstruction()
    {
      var topology = new UTTopology();
      var serverAImpl = new REServerAImpl(RPCFactory.Object, topology, Logger);
      var dispatchA = new REServerAServiceHost(RPCFactory.Object, topology, Logger, serverAImpl);
      var serverA = await dispatchA.Publish();
      await using(serverA)
      {
        var serverBImpl = new REServerBImplEarlyConstruction(RPCFactory.Object, topology, Logger);
        await serverBImpl.ConnectA();
        await using(serverBImpl)
        {
          var dispatchB = new REServerBServiceHost(RPCFactory.Object, topology, Logger, serverBImpl);
          var serverB = await dispatchB.Publish();
          await using(serverB)
          {
            var client = new REServerAClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 4009, ConnectTimeout = 4010 });
            await client.Connect();
            await using(client)
            {
              var value = await client.GetOuterValueAsync('A', 2);
              await Assert.That(value).IsEqualTo("ABC");
            }
          }
        }
      }
    }
  }
}
