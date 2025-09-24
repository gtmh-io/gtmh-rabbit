using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFields000
  {
    [GTField]
    public string StringValue { get; set; } = "StringValueDefault";
    public HasGTFields000(string a_StringValue)
    {
      StringValue = a_StringValue;
    }
  }
}
