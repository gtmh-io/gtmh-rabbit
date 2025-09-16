using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit
{
  public class TransientQueueTopology<M> : IMQTopology<M>
  {
    public bool ConsumerPersists => false;
    public virtual string ExchangeName
    {
      get
      {
        var rval = typeof(M).FullName;
        if ( rval == null ) throw new InvalidOperationException("Invalid type for basic MQ topology");
        return rval;
      }
    }

    public virtual string ConsumerQueueName(string? a_RoutingKey)
    {
      return $"{typeof(M).FullName}::{Environment.MachineName}::{Guid.NewGuid()}::{a_RoutingKey??"*"}";
    }
  }
}
