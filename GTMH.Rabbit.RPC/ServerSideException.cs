using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public class ServerSideException : RPCException
  {
    public string RemoteExceptionData { get; }
    public ServerSideException(string a_ExceptionData) : base("Server Side Exception")
    {
      RemoteExceptionData = a_ExceptionData;
    }
    public override string ToString()
    {
      return base.ToString();
    }
  }
}
