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
    public readonly IFieldData[] Fields;
    public readonly bool HasGTParent;
    public readonly bool CustomConstructor;

    public S11nClassDefn(List<string> a_Usings, string a_NS, string a_Visibility, string a_ClassName, List<IFieldData> attrs, bool a_HasGTParent, bool a_CustomConstructor)
    {
      this.Usings = a_Usings.ToArray();
      this.Namespace = a_NS;
      this.Visibility = a_Visibility;
      this.ClassName = a_ClassName;
      this.Fields = attrs.ToArray();
      this.HasGTParent = a_HasGTParent;
      this.CustomConstructor = a_CustomConstructor;
    }

    public class GTFieldAttrs
    {
      internal string AKA = null;
    }

    public interface IFieldData
    {
      void WriteGather(Code code, string a_Args);
      void WriteInitialisation(Code code);
    }

    public class EnumField : IFieldData
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

      public void WriteGather(Code code, string a_Args)
      {
        code.WriteLine($"{a_Args}.Add(\"{this.Name}\", {this.Name}.ToString());");
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
            code.WriteLine("var NExist=Guid.NewGuid().ToString();");
            code.WriteLine($"var tmp = a_Args.GetValue(\"{this.Name}\", NExist);");
            code.WriteLine($"if ( tmp == NExist ) tmp = a_Args.GetValue(\"{Attrs.AKA}\", {this.Name}.ToString());");
          }

          code.WriteLine($"if( Enum.TryParse<{this.Type}>(tmp, out {this.Type} _tmp)) {this.Name}=_tmp;");
          code.WriteLine($"else throw new ArgumentException(\"Could not convert to {this.Type}\");");
        }
        code.WriteLine("}");
      }
    }

    public class TryParseField : IFieldData
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

      public void WriteGather(Code code, string a_Args)
      {
        switch(Type)
        {
          case "String":
          {
            code.WriteLine($"{a_Args}.Add(\"{this.Name}\", {this.Name});");
            break;
          }
          default:
          {
            code.WriteLine($"{a_Args}.Add(\"{this.Name}\", {this.Name}.ToString());");
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
                code.WriteLine("var NExist=Guid.NewGuid().ToString();");
                // try for the new field name first
                code.WriteLine($"var tmp=a_Args.GetValue(\"{this.Name}\", NExist);");
                code.WriteLine($"if(tmp==NExist) tmp=a_Args.GetValue(\"{Attrs.AKA}\", this.{this.Name});");
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
                code.WriteLine("var NExist=Guid.NewGuid().ToString();");
                // try for the new field name first
                code.WriteLine($"var tmp=a_Args.GetValue(\"{this.Name}\", NExist);");
                code.WriteLine($"if(tmp==NExist) tmp=a_Args.GetValue(\"{Attrs.AKA}\", this.{this.Name}.ToString());");
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
}
