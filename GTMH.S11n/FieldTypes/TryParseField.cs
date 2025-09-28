using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
    public class TryParseField : IFieldType
    {
      public readonly string Type;
      public readonly string Name;
      public readonly GTFieldAttrs Attrs;
      public TryParseField(string a_Name, String a_Type, GTFieldAttrs a_Attrs)
      {
        Name = a_Name;
        Type =a_Type;
        Attrs = a_Attrs;
      }

      public void WriteGather(Code code)
      {
        switch(Type)
        {
          case "String":
          {
            code.WriteLine($"a_Args.Add(\"{this.Name}\", {this.Name});");
            break;
          }
          default:
          {
            code.WriteLine($"a_Args.Add(\"{this.Name}\", {this.Name}.ToString());");
            break;
          }
        }
      }

      public void WriteInitialisation(Code code)
      {
        switch(Type)
        {
          case "String":
          {
            if(Attrs.AKA == null)
            {
              code.WriteLine($"this.{Name}=a_Args.GetValue(\"{this.Name}\", {this.Name});");
            }
            else
            {
              code.WriteLine("{");
              using(code.Indent())
              {
                // try for the new field name first
                code.WriteLine($"var tmp=a_Args.GetValue(\"{this.Name}\", GTInitArgs.NoValue);");
                code.WriteLine($"if(tmp==GTInitArgs.NoValue) tmp=a_Args.GetValue(\"{Attrs.AKA}\", this.{this.Name});");
                code.WriteLine($"this.{Name}=tmp;");
              }
              code.WriteLine("}");
            }
            break;
          }
          default:
          {
            code.WriteLine("{");
            using(code.Indent())
            {
              if(Attrs.AKA == null)
              {
                code.WriteLine($"var tmp = a_Args.GetValue(\"{this.Name}\", this.{this.Name}.ToString());");
              }
              else
              {
                // try for the new field name first
                code.WriteLine($"var tmp=a_Args.GetValue(\"{this.Name}\", GTInitArgs.NoValue);");
                code.WriteLine($"if(tmp==GTInitArgs.NoValue) tmp=a_Args.GetValue(\"{Attrs.AKA}\", this.{this.Name}.ToString());");
              }
              code.WriteLine($"this.{this.Name}={this.Type}.Parse(tmp);");
            }
            code.WriteLine("}");
            break;
          }
        }
      }
    }
}
