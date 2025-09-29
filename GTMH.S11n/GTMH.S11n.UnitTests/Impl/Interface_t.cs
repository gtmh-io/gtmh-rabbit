using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public interface Interface_t
  {
    string Value { get; }
  }
  public partial class InterfaceImplA : Interface_t
  {
    [GTS11n]
    public string Value { get; }
    public InterfaceImplA(string a_Value)
    {
      Value = $"{a_Value}_A";
    }
  }
  public partial class InterfaceImplB : Interface_t
  {
    [GTS11n]
    public string Value { get; }
    public InterfaceImplB(string a_Value)
    {
      Value = $"{a_Value}_B";
    }
  }
}
