using GTMH.GRPC.Discovery;

using System;
using System.Collections.Generic;
using System.Text;

using static GrpcCommon.HelloWorld;

namespace GrpcCommon
{
  public partial class HelloWorldDiscoverable : Discoverable<HelloWorldClient>
  {
  }
}
