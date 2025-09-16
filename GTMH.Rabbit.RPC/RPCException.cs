using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public class RPCException : Exception
  {
    public RPCException(string a_Msg) : base(a_Msg) { }

  }
}
