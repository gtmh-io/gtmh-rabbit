using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
  internal class InstanceField : IFieldType
  {
    public readonly string Name;
    public readonly string InstanceMemberName;
    public readonly string InterfaceType;
    private readonly GTFieldAttrs Attrs;

    public InstanceField(string a_Name, string backingName, string a_InterfaceType, GTFieldAttrs attr)
    {
      Name = a_Name;
      InstanceMemberName = backingName;
      InterfaceType = a_InterfaceType;
      this.Attrs = attr;
    }

    public void WriteGather(Code code)
    {
      code.WriteLine($"a_Args.Add(\"{Name}\", {InstanceMemberName}.GetType().FullName);");
      code.WriteLine($"using(a_Args.Context(\"{Name}\")) a_Args.S11nGather(this.{InstanceMemberName});");
    }

    public void WriteInitialisation(Code code)
    {
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine($"var paramName = \"{Name}\";");
        if(Attrs.AKA != null)
        {
          code.WriteLine($"var tmp = a_Args.GetValue(paramName, GTInitArgs.NoValue);");
          code.WriteLine("if (tmp == GTInitArgs.NoValue )");
          code.WriteLine("{");
          using(code.Indent())
          {
            code.WriteLine($"paramName=\"{Attrs.AKA}\";");
            code.WriteLine($"this.{Name}=a_Args.GetValue(paramName, this.{Name});");
          }
          code.WriteLine("}");
          code.WriteLine("else");
          code.WriteLine("{");
          using(code.Indent())
          {
            code.WriteLine($"this.{Name}=tmp;");
          }
          code.WriteLine("}");
        }
        else
        {
          code.WriteLine($"this.{Name}=a_Args.GetValue(paramName, this.{Name});");
        }
        code.WriteLine($"var type=Type.GetType(this.{Name});");
        code.WriteLine($"if (type==null) throw new S11nException($\"Couldn't find type '{{this.{this.Name}}}'\");");
        code.WriteLine("var constructor = type.GetConstructor( BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(GTMH.S11n.IGTInitArgs) }, null);");
        code.WriteLine($"if (constructor==null) throw new S11nException($\"Type '{{this.{this.Name}}}' has no suitable constructor\");");
        code.WriteLine($"using(a_Args.Context(paramName)) this.{InstanceMemberName}=({InterfaceType})constructor.Invoke(new object[] {{ a_Args }});");
      }
      code.WriteLine("}");
    }
  }
}
