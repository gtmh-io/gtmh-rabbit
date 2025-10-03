using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using GTMH.IDL;

namespace GTMH.Rabbit.RPC.UnitTests
{
  [RPCInterface]
  public interface ITCServer
  {
    [RPCMethod]
    public ValueTask<long> ExecuteAsync(int a_Id);
  }
  class TCServerImpl : ITCServer
  {
    long CurrentCount = 0;
    public async ValueTask<long> ExecuteAsync(int a_Id)
    {
      Interlocked.Increment(ref CurrentCount);
      try
      {
        await Task.Delay(100);
        var rval = Interlocked.Read(ref CurrentCount);
        return rval;

      }
      finally
      {
        Interlocked.Decrement(ref CurrentCount);
      }
    }
  }

  public class TestConcurrency : RPCUnitTests
  {
    [Test]
    public async ValueTask TestConcurrentCallsUnlimited()
    {
      var topology = new UTTopology();
      var serverImpl = new TCServerImpl();
      var dispatch = new TCServerServiceHost(RPCFactory.Object, topology, Logger, serverImpl);
      var server = await dispatch.Publish();
      int NumConcurrent = 10;
      int NumCalls = 10;
      await using(server)
      {
        var clients = Enumerable.Range(0, NumConcurrent).Select( _=> new TCServerClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 4000, ConnectTimeout = 4000 })).ToArray();
        foreach(var client in clients)
        {
          await client.Connect();
        }
        try
        {
          var callCounts = new ConcurrentDictionary<TCServerClient, long>();
          var tasks = new List<Task>();
          foreach(var client in clients)
          {
            tasks.Add(Task.Run(async ()=>
            {
              for(int i = 0; i != NumCalls; ++i)
              {
                var cc = await client.ExecuteAsync(i);
                callCounts.AddOrUpdate(client, cc, (c,curr)=> Math.Max(curr, cc));
              }
            }));
          }
          Task.WaitAll(tasks);
          var maxConcurrent = callCounts.Values.Max();
          await Assert.That(maxConcurrent).IsEqualTo(NumConcurrent);
        }
        finally
        {
          foreach(var client in clients)
          {
            await client.DisposeAsync();
          }
        }
      }
    }
    [Test]
    public async ValueTask TestConcurrentCallsLimited()
    {
      ushort Limit = 5;
      var topology = new UTTopology();
      var serverImpl = new TCServerImpl();
      RPCFactory.Setup(_=>_.ServerMaxConcurrency).Returns(Limit);
      var dispatch = new TCServerServiceHost(RPCFactory.Object, topology, Logger, serverImpl);
      var server = await dispatch.Publish();
      int NumConcurrent = 10;
      int NumCalls = 10;
      await using(server)
      {
        var clients = Enumerable.Range(0, NumConcurrent).Select( _=> new TCServerClient(RPCFactory.Object, topology, Logger, new RPCClientConfig { CallTimeout = 4000, ConnectTimeout = 4000 })).ToArray();
        foreach(var client in clients)
        {
          await client.Connect();
        }
        try
        {
          var callCounts = new ConcurrentDictionary<TCServerClient, long>();
          var tasks = new List<Task>();
          foreach(var client in clients)
          {
            tasks.Add(Task.Run(async ()=>
            {
              for(int i = 0; i != NumCalls; ++i)
              {
                var cc = await client.ExecuteAsync(i);
                callCounts.AddOrUpdate(client, cc, (c,curr)=> Math.Max(curr, cc));
              }
            }));
          }
          Task.WaitAll(tasks);
          var maxConcurrent = callCounts.Values.Max();
          await Assert.That(maxConcurrent).IsEqualTo(Limit);
        }
        finally
        {
          foreach(var client in clients)
          {
            await client.DisposeAsync();
          }
        }
      }
    }
  }
}
