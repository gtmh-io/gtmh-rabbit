using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery.AddressResolution
{
  public class LoopbackAddressResolution : IAddressResolver
  {
    public string Resolve(string a_URL)
    {
      var uri = new Uri(a_URL);
      if(uri.Host == "[::]" || uri.Host == "0.0.0.0")
      {
        var builder = new UriBuilder(uri) { Host = "127.0.0.1" };
        var rval = builder.ToString();
        if ( rval.EndsWith('/') ) return rval.Substring(0, rval.Length-1);
        else return rval;
      }
      else
      {
        return a_URL;
      }
    }
  }
}
