using Microsoft.Extensions.Logging;

using Moq;

// https://tunit.dev/

using System;
using System.Collections.Generic;
using System.Text;

using GTMH.IDL;

namespace GTMH.Rabbit.RPC.UnitTests
{
  [RPCInterface]
  public interface ISynchronousInterface
  {
    [RPCMethod]
    string TestMethod(string a_Arg);
    [RPCMethod]
    void TestVoidA(string a_Arg);
    [RPCMethod]
    void TestVoidB();
  }


  public class SynchronousInterfaceTests : RPCUnitTests
  {
    [Test]
    public async ValueTask TestSynchronous()
    {
      var id = Guid.NewGuid().ToString();
      Mock<ISynchronousInterface> serverImpl = new ();
      serverImpl.Setup(_=>_.TestMethod(It.IsAny<string>())).Returns( (string arg)=>$"{arg}::{id}" );
      UTTopology topology = new();
      var serverHost = new SynchronousInterfaceServiceHost(RPCFactory.Object, topology, Logger, serverImpl.Object); 
      var server = await serverHost.Publish();
      await using(server)
      {
        var client = new SynchronousInterfaceClient(RPCFactory.Object, topology, Logger, null);
        await client.Connect();
        await using(client)
        {
          var localId = Guid.NewGuid().ToString();
          var rval = client.TestMethod(localId);
          await Assert.That(rval).IsEqualTo($"{localId}::{id}");
          client.TestVoidA(localId);
          client.TestVoidB();
        }
      }
      serverImpl.Verify(_=>_.TestVoidA(It.IsAny<string>()), Times.Once());
      serverImpl.Verify(_=>_.TestVoidB(), Times.Once());
    }
    [Test]
    public async ValueTask TestAsynchronous()
    {
      var id = Guid.NewGuid().ToString();
      Mock<ISynchronousInterface> serverImpl = new ();
      serverImpl.Setup(_=>_.TestMethod(It.IsAny<string>())).Returns( (string arg)=>$"{arg}::{id}" );
      UTTopology topology = new();
      var serverHost = new SynchronousInterfaceServiceHost(RPCFactory.Object, topology, Logger, serverImpl.Object); 
      var server = await serverHost.Publish();
      await using(server)
      {
        var client = new SynchronousInterfaceClient(RPCFactory.Object, topology, Logger, null);
        await client.Connect();
        await using(client)
        {
          var localId = Guid.NewGuid().ToString();
          var rval = await client.TestMethodAsync(localId);
          await Assert.That(rval).IsEqualTo($"{localId}::{id}");
          await client.TestVoidAAsync(localId);
          await client.TestVoidBAsync();

        }
      }
      serverImpl.Verify(_=>_.TestVoidA(It.IsAny<string>()), Times.Once());
      serverImpl.Verify(_=>_.TestVoidB(), Times.Once());
    }
    [Test]
    public async ValueTask TestServerThrows()
    {
      Mock<ISynchronousInterface> serverImpl = new ();
      serverImpl.Setup(_=>_.TestMethod(It.IsAny<string>())).Throws<Exception>();
      UTTopology topology = new();
      var serverHost = new SynchronousInterfaceServiceHost(RPCFactory.Object, topology, Logger, serverImpl.Object); 
      var server = await serverHost.Publish();
      await using(server)
      {
        var client = new SynchronousInterfaceClient(RPCFactory.Object, topology, Logger, null);
        await client.Connect();
        await using(client)
        {
          Assert.Throws<ServerSideException>(()=>client.TestMethod("bla"));
        }
      }
    }
    [Test]
    public async ValueTask TestServerThrowsAsync()
    {
      Mock<ISynchronousInterface> serverImpl = new ();
      serverImpl.Setup(_=>_.TestMethod(It.IsAny<string>())).Throws<Exception>();
      UTTopology topology = new();
      var serverHost = new SynchronousInterfaceServiceHost(RPCFactory.Object, topology, Logger, serverImpl.Object); 
      var server = await serverHost.Publish();
      await using(server)
      {
        var client = new SynchronousInterfaceClient(RPCFactory.Object, topology, Logger, null);
        await client.Connect();
        await using(client)
        {
          await Assert.ThrowsAsync<ServerSideException>(async ()=> await client.TestMethodAsync("bla"));
        }
      }
    }
    [Test]
    public async ValueTask TestCallTimeout()
    {
      Mock<ISynchronousInterface> serverImpl = new ();
      serverImpl.Setup(_=>_.TestMethod(It.IsAny<string>())).Returns((string arg)=>
      {
        Thread.Sleep(1000);
        return "sorry";
      });
      UTTopology topology = new();
      var serverHost = new SynchronousInterfaceServiceHost(RPCFactory.Object, topology, Logger, serverImpl.Object); 
      var server = await serverHost.Publish();
      await using(server)
      {
        var client = new SynchronousInterfaceClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 100 });
        await using(client)
        {
          await client.Connect();
          Assert.Throws<AggregateException>(()=>client.TestMethod("bla")); // if using the non-async interface the exception will be an aggregate
          try
          {
            client.TestMethod("bla");
          }
          catch(AggregateException e)
          {
            var toe = e.InnerException as RPCTimeout;
            await Assert.That(toe).IsNotNull();
          }
        }
      }
    }
    [Test]
    public async ValueTask TestCallTimeoutAsync()
    {
      Mock<ISynchronousInterface> serverImpl = new ();
      serverImpl.Setup(_=>_.TestMethod(It.IsAny<string>())).Returns((string arg)=>
      {
        Thread.Sleep(1000);
        return "sorry";
      });
      UTTopology topology = new();
      var serverHost = new SynchronousInterfaceServiceHost(RPCFactory.Object, topology, Logger, serverImpl.Object); 
      var server = await serverHost.Publish();
      await using(server)
      {
        var client = new SynchronousInterfaceClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 100 });
        await using(client)
        {
          await client.Connect();
          await Assert.ThrowsAsync<RPCTimeout>(async ()=> await client.TestMethodAsync("bla"));
        }
      }
    }
    [Test]
    public async ValueTask TestConnectTimeout()
    {
      UTTopology topology = new();
      var client = new SynchronousInterfaceClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { ConnectTimeout = 100 });
      await using(client)
      {
        await Assert.ThrowsAsync<RPCTimeout>( async ()=> await client.Connect() );
      }
    }
  }
}
