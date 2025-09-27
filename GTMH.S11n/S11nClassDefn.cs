using GTMH.S11n.FieldTypes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    public readonly IFieldType[] Fields;
    public readonly bool HasGTParent;
    public readonly bool CustomConstructor;

    public S11nClassDefn(List<string> a_Usings, string a_NS, string a_Visibility, string a_ClassName, List<IFieldType> attrs, bool a_HasGTParent, bool a_CustomConstructor)
    {
      this.Usings = a_Usings.ToArray();
      this.Namespace = a_NS;
      this.Visibility = a_Visibility;
      this.ClassName = a_ClassName;
      this.Fields = attrs.ToArray();
      this.HasGTParent = a_HasGTParent;
      this.CustomConstructor = a_CustomConstructor;
    }

  }
}
