using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsAsProperties
  {
    [GTField]
    public string StringValue { get; set; } = "StringValueDefault";
    [GTField]
    public int IntValue { get; set; } = 69;
    public enum Value_t { ValueA = 1, ValueB = 2 };
    [GTField]
    public Value_t EnumValue { get; set; } = Value_t.ValueA;
    public HasGTFieldsAsProperties(string a_StringValue, int a_IntValue, Value_t a_EnumValue)
    {
      StringValue = a_StringValue;
      IntValue = a_IntValue;
      EnumValue = a_EnumValue;
    }
  }
}
