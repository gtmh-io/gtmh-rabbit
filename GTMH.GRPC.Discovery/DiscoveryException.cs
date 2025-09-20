using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public class DiscoveryException : Exception
  {
    public DiscoveryException(string message) : base(message) { }
  }
}
