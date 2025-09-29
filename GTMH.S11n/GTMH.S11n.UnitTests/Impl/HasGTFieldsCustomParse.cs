using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsCustomParse
  {
    public struct CustomType
    {
      public string Code;
    }
    [GTS11n(Parse=nameof(Parse), DeParse=nameof(Deparse))]
    public readonly CustomType Id = new CustomType { Code="XYZ" };

    [GTS11n(Parse=nameof(Parse), DeParse=nameof(Deparse), AKA ="OldId2")]
    public readonly CustomType Id2 = new CustomType { Code="XYZ" };

    public HasGTFieldsCustomParse(string a_Code, string a_Code2)
    {
      Id = new CustomType { Code=a_Code };
      Id2 = new CustomType { Code = a_Code2 };
    }

    static string Deparse(CustomType a_Type) =>a_Type.Code;
    static CustomType Parse(string a_Code) => new CustomType {Code=a_Code};
  }
}
