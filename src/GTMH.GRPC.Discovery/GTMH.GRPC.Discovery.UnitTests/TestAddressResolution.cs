using GTMH.GRPC.Discovery.AddressResolution;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery.UnitTests
{
  public class TestAddressResolution
  {
    [Test]
    public async ValueTask TestLoopbackAddressResolution()
    {
      await Assert.That(new LoopbackAddressResolution().Resolve("http://[::]:51023")).IsEqualTo("http://127.0.0.1:51023");
      await Assert.That(new LoopbackAddressResolution().Resolve("http://0.0.0.0:51024")).IsEqualTo("http://127.0.0.1:51024");
      await Assert.That(new LoopbackAddressResolution().Resolve("https://0.0.0.0:51025")).IsEqualTo("https://127.0.0.1:51025");

      await Assert.That(new LoopbackAddressResolution().Resolve("http://1.2.3.4:51024")).IsEqualTo("http://1.2.3.4:51024");
    }
  }
}
