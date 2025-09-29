using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
  internal class InstanceField : IFieldType
  {
    public readonly string Name;
    public readonly string InterfaceType;
    private readonly GTFieldAttrs Attrs;

    public InstanceField(string a_Name, string a_InterfaceType, GTFieldAttrs attr)
    {
      Name = a_Name;
      InterfaceType = a_InterfaceType;
      this.Attrs = attr;
    }

    public void WriteGather(Code code)
    {
      // if change here note to change in InstanceArrayField WriteGather
      if(Attrs.DeParse != null)
      {
        code.WriteLine($"if ( this.{Name} !=null) {Attrs.DeParse}(\"{Name}\", this.{Name}, a_Args);");
      }
      else
      {
        code.WriteLine($"a_Args.Add(\"{Name}\", a_Args.DisolveType(this.{Name}));");
        code.WriteLine($"using(a_Args.Context(\"{Name}\")) if ( this.{Name} !=null) a_Args.S11nGather(this.{Name});");
      }
    }

    public void WriteInitialisation(Code code)
    {
      // if change here note to change in InstanceArrayField WriteInitialisation
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine($"var paramName = \"{Name}\";");
        if(Attrs.AKA != null)
        {
          code.WriteLine($"var instanceDefn = a_Args.GetValue(paramName, GTInitArgs.NoValue);");
          code.WriteLine("if (instanceDefn == GTInitArgs.NoValue )");
          code.WriteLine("{");
          using(code.Indent())
          {
            code.WriteLine($"paramName=\"{Attrs.AKA}\";");
            code.WriteLine($"instanceDefn=a_Args.GetValue(paramName, GTInitArgs.NoValue);");
          }
          code.WriteLine("}");
        }
        else
        {
          code.WriteLine($"var instanceDefn=a_Args.GetValue(paramName, GTInitArgs.NoValue);");
        }
        code.WriteLine($"if ( instanceDefn == GTInitArgs.NoValue || string.IsNullOrEmpty(instanceDefn))");
        code.WriteLine("{");
        using(code.Indent())
        {
          if(Attrs.Required)
          {
            code.WriteLine($"throw new S11nException(\"'{this.Name}' is required but not configured\");");
          }
          else
          {
            code.WriteLine($"this.{Name}=null;");
          }
        }
        code.WriteLine("}");
        code.WriteLine("else");
        code.WriteLine("{");
        using(code.Indent())
        {
          code.WriteLine($"var type=a_Args.ResolveType(instanceDefn);");
          code.WriteLine($"if (type==null) throw new S11nException($\"Couldn't find type '{{instanceDefn}}'\");");
          if(Attrs.Parse != null)
          {
            code.WriteLine($"using(a_Args.Context(paramName)) this.{Name}={Attrs.Parse}(type, a_Args);");
          }
          else
          {
            code.WriteLine("var constructor = type.GetConstructor( BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(GTMH.S11n.IGTInitArgs) }, null);");
            code.WriteLine($"if (constructor==null) throw new S11nException($\"Type '{{this.{this.Name}}}' has no suitable constructor\");");
            code.WriteLine($"using(a_Args.Context(paramName)) this.{Name}=({InterfaceType})constructor.Invoke(new object[] {{ a_Args }});");
          }
        }
        code.WriteLine("}");
      }
      code.WriteLine("}");
    }
  }
}
