using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsCustomConstructor
  {
    [GTS11n]
    public string StringValue { get; private set; } = "AStringValue";
    public HasGTFieldsCustomConstructor(string a_StringValue)
    {
      StringValue = a_StringValue;
    }
    [GTS11nInit]
    public HasGTFieldsCustomConstructor(IGTInitArgs a_Args)
    {
      this.SetS11n(a_Args);
    }
  }
}
