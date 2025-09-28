using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
  public class EnumField : IFieldType
  {
    public readonly string Name;
    public readonly string Type;
    public readonly GTFieldAttrs Attrs;
    public EnumField(string a_Name, string a_Type, GTFieldAttrs a_Attrs)
    {
      this.Name = a_Name;
      Type = a_Type;
      Attrs = a_Attrs;
    }

    public void WriteGather(Code code)
    {
      code.WriteLine($"a_Args.Add(\"{this.Name}\", {this.Name}.ToString());");
    }

    public void WriteInitialisation(Code code)
    {
      code.WriteLine("{");
      using(code.Indent())
      {
        if(Attrs.AKA == null)
        {
          code.WriteLine($"var tmp = a_Args.GetValue(\"{this.Name}\", {this.Name}.ToString());");
        }
        else
        {
          code.WriteLine($"var tmp = a_Args.GetValue(\"{this.Name}\", GTInitArgs.NoValue);");
          code.WriteLine($"if ( tmp == GTInitArgs.NoValue ) tmp = a_Args.GetValue(\"{Attrs.AKA}\", {this.Name}.ToString());");
        }

        code.WriteLine($"if( Enum.TryParse<{this.Type}>(tmp, out {this.Type} _tmp)) {this.Name}=_tmp;");
        code.WriteLine($"else throw new ArgumentException(\"Could not convert to {this.Type}\");");
      }
      code.WriteLine("}");
    }
  }
}
