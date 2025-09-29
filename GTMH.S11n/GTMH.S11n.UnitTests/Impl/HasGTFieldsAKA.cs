using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsAKA
  {
    public enum Enum_t { ValueA = 1, ValueB = 2, ValueC = 3 }
    [GTS11n(AKA="OldStringProperty")]
    public readonly string NewStringProperty = "StringValue";
    [GTS11n(AKA ="OldIntProperty")]
    public readonly int NewIntProperty = 69;
    [GTS11n(AKA ="OldEnumProperty")]
    public readonly Enum_t NewEnumProperty = Enum_t.ValueA;
    public HasGTFieldsAKA(string a_StringValue, int a_IntValue, Enum_t a_EnumValue)
    {
      NewStringProperty = a_StringValue;
      NewIntProperty = a_IntValue;
    }
  }
}
