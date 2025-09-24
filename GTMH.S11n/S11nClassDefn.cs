using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public class S11nClassDefn
  {
    public readonly string[] Usings;
    public readonly string Namespace;
    public readonly string Visibility;
    public readonly string ClassName;
    public readonly AttrData[] GTFields;

    public S11nClassDefn(List<string> a_Usings, string a_NS, string a_Visibility, string a_ClassName, List<AttrData> attrs)
    {
      this.Usings = a_Usings.ToArray();
      this.Namespace = a_NS;
      this.Visibility = a_Visibility;
      this.ClassName = a_ClassName;
      this.GTFields = attrs.ToArray();
    }

    public class AttrData
    {
      public readonly string Name;
      public AttrData(string a_Name)
      {
        Name = a_Name;
      }

    }
  }
}
