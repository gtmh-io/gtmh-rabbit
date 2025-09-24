using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests
{
  public class GTFieldsTests
  {
    [Test]
    public async ValueTask TestBasic()
    {
      await Assert.That(Math.Abs(0)).IsGreaterThan(1);
    }
  }
}
