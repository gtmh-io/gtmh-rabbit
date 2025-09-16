using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public interface IRPCTopology
  {
    string QueueName(string a_InterfaceType);
    ValueTask AddAsync(IRPCServer a_Server);
    /// <summary>
    /// This should be a no-op if already removed
    /// </summary>
    ValueTask RemoveAsync(IRPCServer a_Server);
    ValueTask<string> FindAsync(string a_InterfaceType);
  }
}
