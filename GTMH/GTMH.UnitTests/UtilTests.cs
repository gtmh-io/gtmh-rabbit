using System;
using System.Collections.Generic;
using System.Text;

using GTMH.Util;

namespace GTMH.UnitTests
{
  public class UtilTests
  {
    [Test]
    public async ValueTask TestCmdLineArgs()
    {
      await Assert.That(new[] { "-arg=value" }.GetCmdLine("arg", "not_this")).IsEqualTo("value");
      await Assert.That(new[] { "--arg=value" }.GetCmdLine("arg", "not_this")).IsEqualTo("value");
      await Assert.That(new[] { "-argx=value" }.GetCmdLine("arg", "not_this")).IsEqualTo("not_this");

      // strips leading dashes
      await Assert.That(new[] { "-arg=value" }.GetCmdLine("-arg", "not_this")).IsEqualTo("value");
      await Assert.That(new[] { "-arg=value" }.GetCmdLine("--arg", "not_this")).IsEqualTo("value");

      await Assert.That(new[] { "-arg=value" }.GetCmdLine("arg")).IsEqualTo("value");
      await Assert.That(new[] { "--arg=value" }.GetCmdLine("arg")).IsEqualTo("value");
      // strips leading dashes
      await Assert.That(new[] { "-arg=value" }.GetCmdLine("-arg")).IsEqualTo("value");
      await Assert.That(new[] { "--arg=value" }.GetCmdLine("--arg")).IsEqualTo("value");

      await Assert.That(new[] { "-argx=value" }.GetCmdLine("arg")).IsNull();

    }
  }
}
