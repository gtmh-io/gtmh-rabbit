using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.UnitTests
{
  class UTDefaultTopology<M> : IMQTopology<M>
  {
    public readonly string UniqueName  = Guid.NewGuid().ToString();
    public string ExchangeName => UniqueName;
    public string ConsumerQueueName(string? a_RoutingKey) => Guid.NewGuid().ToString();
    public bool ConsumerPersists => false;
  }
}
