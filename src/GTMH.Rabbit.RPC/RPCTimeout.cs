using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public class RPCTimeout : RPCException
  {
    public RPCTimeout(string message) : base(message) { }
  }
}
