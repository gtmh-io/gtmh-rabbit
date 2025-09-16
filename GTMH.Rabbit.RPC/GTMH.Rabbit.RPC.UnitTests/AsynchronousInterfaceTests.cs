using Moq;

using System;
using System.Collections.Generic;
using System.Text;

using GTMH.IDL;

namespace GTMH.Rabbit.RPC.UnitTests
{
  [RPCInterface]
  public interface IAsynchronousInterface
  {
    [RPCMethod]
    Task<string> TaskTestMethodAsync(string a_Arg);
    [RPCMethod]
    ValueTask<string> ValueTaskTestMethodAsync(string a_Arg);
    [RPCMethod]
    ValueTask ValueTaskTestVoidAAsync(string a_Arg);
    [RPCMethod]
    ValueTask ValueTaskTestVoidBAsync();
    [RPCMethod]
    Task TaskTestVoidAAsync(string a_Arg);
    [RPCMethod]
    Task TaskTestVoidBAsync();
  }
  public class AsynchronousInterfaceTests : RPCUnitTests
  {
    [Test]
    public async ValueTask TestAsynchronous()
    {
      var id = Guid.NewGuid().ToString();
      Mock<IAsynchronousInterface> serverImpl = new ();
      serverImpl.Setup(_=>_.TaskTestMethodAsync(It.IsAny<string>())).Returns( (string arg)=>Task.FromResult($"{arg}::{id}") );
      serverImpl.Setup(_=>_.ValueTaskTestMethodAsync(It.IsAny<string>())).Returns( (string arg)=>ValueTask.FromResult($"{arg}::{id}") );
      UTTopology topology = new();
      var serverHost = new AsynchronousInterfaceDispatch(RPCFactory.Object, topology, Logger, serverImpl.Object); 
      var server = await serverHost.Publish();
      await using(server)
      {
        var client = new AsynchronousInterfaceClient(RPCFactory.Object, topology, Logger, null);
        await client.Connect();
        await using(client)
        {
          var localId = Guid.NewGuid().ToString();
          {
            var rval = await client.TaskTestMethodAsync(localId);
            await Assert.That(rval).IsEqualTo($"{localId}::{id}");
          }
          {
            var rval = await client.ValueTaskTestMethodAsync(localId);
            await Assert.That(rval).IsEqualTo($"{localId}::{id}");
          }
          await client.ValueTaskTestVoidAAsync(localId);
          await client.ValueTaskTestVoidBAsync();
          await client.TaskTestVoidAAsync(localId);
          await client.TaskTestVoidBAsync();
        }
      }
      serverImpl.Verify(_=>_.ValueTaskTestVoidAAsync(It.IsAny<string>()), Times.Once());
      serverImpl.Verify(_=>_.ValueTaskTestVoidBAsync(), Times.Once());
      serverImpl.Verify(_=>_.TaskTestVoidAAsync(It.IsAny<string>()), Times.Once());
      serverImpl.Verify(_=>_.TaskTestVoidBAsync(), Times.Once());
    }

  }
}
