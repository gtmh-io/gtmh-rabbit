using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsAsROFields
  {
    [GTS11n]
    public readonly string StringValue  = "StringValueDefault";
    [GTS11n]
    public readonly int IntValue = 69;
    public enum Value_t { ValueA = 1, ValueB = 2 };
    [GTS11n]
    public readonly Value_t EnumValue = Value_t.ValueA;
    public HasGTFieldsAsROFields(string a_StringValue, int a_IntValue, Value_t a_EnumValue)
    {
      StringValue = a_StringValue;
      IntValue = a_IntValue;
      EnumValue = a_EnumValue;
    }
  }
}
