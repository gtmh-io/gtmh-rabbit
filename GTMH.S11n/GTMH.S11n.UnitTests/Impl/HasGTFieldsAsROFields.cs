using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsAsROFields
  {
    [GTField]
    public readonly string StringValue  = "StringValueDefault";
    [GTField]
    public readonly int IntValue = 69;
    public enum Value_t { ValueA = 1, ValueB = 2 };
    [GTField]
    public readonly Value_t EnumValue = Value_t.ValueA;
    public HasGTFieldsAsROFields(string a_StringValue, int a_IntValue, Value_t a_EnumValue)
    {
      StringValue = a_StringValue;
      IntValue = a_IntValue;
      EnumValue = a_EnumValue;
    }
  }
}
