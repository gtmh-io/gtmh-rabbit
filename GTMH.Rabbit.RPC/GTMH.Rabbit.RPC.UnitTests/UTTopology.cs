using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC.UnitTests
{
  class UTTopology : IRPCTopology
  {
    readonly string UniqueId = Guid.NewGuid().ToString();
    public ValueTask AddAsync(IRPCServer a_Server) => ValueTask.CompletedTask;
    public ValueTask<string> FindAsync(string a_InterfaceType) => ValueTask.FromResult($"{a_InterfaceType}::{UniqueId}");
    public string QueueName(string a_InterfaceType) => $"{a_InterfaceType }::{UniqueId}";
    public ValueTask RemoveAsync(IRPCServer a_Server) => ValueTask.CompletedTask;
  }
}
