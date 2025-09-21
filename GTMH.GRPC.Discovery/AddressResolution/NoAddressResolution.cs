using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery.AddressResolution
{
  public class NoAddressResolution : IAddressResolver
  {
    public string Resolve(string a_Address)=>a_Address;
    
  }
}
