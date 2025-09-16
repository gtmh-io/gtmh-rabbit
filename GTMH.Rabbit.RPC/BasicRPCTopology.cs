using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public class BasicTopology : IRPCTopology
  {
    public string QueueName(string a_InterfaceType) => a_InterfaceType;
    public ValueTask AddAsync(IRPCServer a_Server) => default(ValueTask);
    public ValueTask RemoveAsync(IRPCServer a_Server) => default(ValueTask);
    public ValueTask<string> FindAsync(string a_InterfaceType) => ValueTask.FromResult(a_InterfaceType);
  }
}
