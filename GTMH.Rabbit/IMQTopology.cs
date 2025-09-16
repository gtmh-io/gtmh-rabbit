using System;
using System.Collections.Generic;
using System.Text;

namespace Tofye.IMQ
{
  public interface IMQTopology<M>
  {
    string ExchangeName { get; }
    string ConsumerQueueName(string? a_RoutingKey);
    bool ConsumerPersists { get; }
  }
}
