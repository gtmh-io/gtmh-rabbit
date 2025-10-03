using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public interface IAddressResolver
  {
    public string Resolve(string a_URL);
  }
}
