using System;
using System.Collections.Generic;
using System.Text;

namespace Tofye.IMQ
{
  public class PersistentQueueTopology<M> : IMQTopology<M>
  {
    private readonly string Scope;
    public virtual string ExchangeName
    {
      get
      {
        var rval = typeof(M).FullName;
        if ( rval == null ) throw new InvalidOperationException("Invalid type for basic MQ topology");
        return rval;
      }
    }

    public PersistentQueueTopology(string a_Scope) { this.Scope = a_Scope; }

    public virtual string ConsumerQueueName(string? a_RoutingKey)=> $"{typeof(M).FullName}::{Scope}::{a_RoutingKey??"*"}";
    public bool ConsumerPersists=>true;
  }
}
