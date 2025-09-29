using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsBase
  {
    [GTS11n]
    public readonly string BaseStringValue;
    public HasGTFieldsBase(string a_BaseValue)
    {
      BaseStringValue = a_BaseValue;
    }
  }
  public partial class HasGTFieldsDerived : HasGTFieldsBase
  {
    [GTS11n]
    public readonly string DerivedStringValue;
    public HasGTFieldsDerived(string a_DerivedValue, string a_BaseValue) : base(a_BaseValue)
    {
      this.DerivedStringValue = a_DerivedValue;
    }
  }
  public partial class HasNotGTFieldsDerived : HasGTFieldsBase
  {
    public HasNotGTFieldsDerived(string a_BaseValue) : base(a_BaseValue) { }
  }
}
