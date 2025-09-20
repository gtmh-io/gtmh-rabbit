using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  [ProtoContract(SkipConstructor=true)]
  public class DiscoveryResponse
  {
    [ProtoMember(10)]
    public string URI { get; set; }
    public DiscoveryResponse(string a_URI ) { URI = a_URI; }
  }
}
