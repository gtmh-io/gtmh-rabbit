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
    [Test]
    public async ValueTask TestCmdLineArgsEmpty()
    {
      await Assert.That(new[] { "-arg=" }.GetCmdLine("arg")).IsNull();
    }
    [Test]
    public async ValueTask TestCmdLineArgsEmptyWithDefault()
    {
      await Assert.That(new[] { "-arg=" }.GetCmdLine("arg", "default")).IsEqualTo("default");
    }
    [Test]
    public async ValueTask TestCmdLineArgsHasEquals()
    {
      var value = "GTMH:v1:AYy4FAVpUtdWdRXvEJe6QsXK39MFSKsllubT4BoytI7VppvEfSzf1w9UR1smpgnuHeWkHeQJ9B6vwOTsZLBxTAc=";
      await Assert.That(new[] { $"-arg={value}" }.GetCmdLine("arg")).IsEqualTo("GTMH:v1:AYy4FAVpUtdWdRXvEJe6QsXK39MFSKsllubT4BoytI7VppvEfSzf1w9UR1smpgnuHeWkHeQJ9B6vwOTsZLBxTAc=");
    }
  }
}
