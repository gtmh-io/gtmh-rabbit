using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
    internal class CustomField : IFieldType
    {
      private readonly string Name;
      private readonly GTFieldAttrs Attrs;

      public CustomField(string Name, GTFieldAttrs a_Attrs)
      {
        this.Name = Name;
        this.Attrs = a_Attrs;
      }

      public void WriteGather(Code code)
      {
        code.WriteLine($"a_Args.Add(\"{this.Name}\", {Attrs.DeParse}({this.Name}));");
      }

      public void WriteInitialisation(Code code)
      {
        if(Attrs.AKA == null)
        {
          code.WriteLine($"this.{Name}={Attrs.Parse}(a_Args.GetValue(\"{this.Name}\", {Attrs.DeParse}({this.Name})));");
        }
        else
        {
          code.WriteLine("{");
          using(code.Indent())
          {
            code.WriteLine($"var tmp=a_Args.GetValue(\"{this.Name}\", GTInitArgs.NoValue);");
            code.WriteLine($"if(tmp==GTInitArgs.NoValue) tmp=a_Args.GetValue(\"{Attrs.AKA}\", {Attrs.DeParse}(this.{this.Name}));");
            code.WriteLine($"this.{Name}={Attrs.Parse}(tmp);");
          }
          code.WriteLine("}");
        }
      }
    }
}
