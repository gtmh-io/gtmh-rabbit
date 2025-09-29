using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsAsProperties
  {
    [GTS11n]
    public string StringValue { get; set; } = "StringValueDefault";
    [GTS11n]
    public int IntValue { get; set; } = 69;
    public enum Value_t { ValueA = 1, ValueB = 2 };
    [GTS11n]
    public Value_t EnumValue { get; set; } = Value_t.ValueA;
    public HasGTFieldsAsProperties(string a_StringValue, int a_IntValue, Value_t a_EnumValue)
    {
      StringValue = a_StringValue;
      IntValue = a_IntValue;
      EnumValue = a_EnumValue;
    }
  }
}
