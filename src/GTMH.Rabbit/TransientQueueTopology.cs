using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit
{
  public class TransientQueueTopology_t<M> : IMQTopology<M>
  {
    public bool ConsumerPersists => false;
    /// <summary>
    /// This ensures type safety for the exchange and queues connected to it. 
    /// </summary>
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
      // this would do
      // return $"{Guid.NewGuid()}"}";
      // but we prefer to add some context for debugging
      return $"{typeof(M).FullName}::{Environment.MachineName}::{Guid.NewGuid()}::{a_RoutingKey??"*"}";
    }
  }
  public static class TransientQueueTopology
  {
    public static IMQTopology<M> Create<M>() => new TransientQueueTopology_t<M>();

  }
}
