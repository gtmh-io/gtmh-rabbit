using GTMH.S11n.UnitTests.Impl;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests
{
  public class GTFieldsTests
  {
    [Test]
    public async ValueTask TestHasGTFields000()
    {
      var obj = new HasGTFields000("roger");
      await Assert.That(obj.StringValue).IsEqualTo("roger");
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFields000(new GTArgs(s11n));
      await Assert.That(_obj.StringValue).IsEqualTo("roger");
    }
  }
}
