using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFields000
  {
    [GTField]
    public string StringValue { get; set; } = "StringValueDefault";
    [GTField]
    public int IntValue { get; set; } = 69;
    public HasGTFields000(string a_StringValue, int a_IntValue)
    {
      StringValue = a_StringValue;
      IntValue = a_IntValue;
    }
  }
}
