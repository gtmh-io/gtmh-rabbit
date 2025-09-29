using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
  internal class InstanceArrayField : IFieldType
  {
    private readonly string Name;
    private readonly string InterfaceType;
    private readonly GTFieldAttrs Attrs;

    public InstanceArrayField(string a_Name, string a_InstanceType, GTFieldAttrs a_Attrs)
    {
      this.Name = a_Name;
      this.InterfaceType = a_InstanceType;
      this.Attrs = a_Attrs;
    }

    public void WriteGather(Code code)
    {
      code.WriteLine($"if (this.{Name}.IsDefaultOrEmpty)");
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine($"a_Args.Add(\"{Name}.Array-Length\", \"0\");"); // Array-Length can't clash with a member
      }
      code.WriteLine("}");
      code.WriteLine("else");
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine($"a_Args.Add(\"{Name}.Array-Length\", this.{Name}.Length.ToString());");
        code.WriteLine($"for ( var idx = 0 ; idx !=this.{Name}.Length ; ++idx)");
        code.WriteLine("{");
        using(code.Indent())
        {
          // if change here note to change in InstanceField WriteGather
          if(Attrs.DeParse != null)
          {
            code.WriteLine($"if ( this.{Name}[idx] !=null) {Attrs.DeParse}($\"{Name}.{{idx}}\", this.{Name}[idx], a_Args);");
          }
          else
          {
            code.WriteLine($"a_Args.Add($\"{Name}.{{idx}}\", a_Args.DisolveType(this.{Name}[idx]));");
            code.WriteLine($"using(a_Args.Context($\"{Name}.{{idx}}\")) if ( this.{Name}[idx] !=null) a_Args.S11nGather(this.{Name}[idx]);");
          }
        }
        code.WriteLine("}");
      }
      code.WriteLine("}");
    }

    public void WriteInitialisation(Code code)
    {
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine($"var tmp = a_Args.GetValue(\"{Name}.Array-Length\", GTInitArgs.NoValue);");
        if(Attrs.AKA != null)
        {
          code.WriteLine($"if ( tmp == GTInitArgs.NoValue ) tmp=a_Args.GetValue(\"{Attrs.AKA}.Array-Length\", GTInitArgs.NoValue);");

        }
        code.WriteLine($"if ( tmp == GTInitArgs.NoValue ) throw new S11nException(\"'{Name}' marked as array but no length found\");");
        code.WriteLine($"int N;");
        code.WriteLine($"if ( ! int.TryParse(tmp, out N) || N <0 ) throw new S11nException(\"Invalid length={{N}} for '{Name}'\");");
        code.WriteLine("// TODO - do I want a sanity check on length");
        code.WriteLine($"var builder=System.Collections.Immutable.ImmutableArray.CreateBuilder<{InterfaceType}>(N);");
        code.WriteLine("for ( var idx = 0 ; idx != N ; ++idx)");
        code.WriteLine("{");
        using(code.Indent())
        {
          WriteIdxInitialisation(code);
        }
        code.WriteLine("}");
        code.WriteLine($"this.{Name}=builder.ToImmutable();");
      }
      code.WriteLine("}");
    }

    private void WriteIdxInitialisation(Code code)
    {
      code.WriteLine($"var paramName = $\"{Name}.{{idx}}\";");
      if(Attrs.AKA != null)
      {
        code.WriteLine($"var instanceDefn = a_Args.GetValue(paramName, GTInitArgs.NoValue);");
        code.WriteLine("if (instanceDefn == GTInitArgs.NoValue )");
        code.WriteLine("{");
        using(code.Indent())
        {
          code.WriteLine($"paramName=$\"{Attrs.AKA}.{{idx}}\";");
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
          code.WriteLine($"throw new S11nException($\"'{this.Name}.{{idx}}' is required but not configured\");");
        }
        else
        {
          code.WriteLine($"builder.Add(null);");
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
          code.WriteLine($"using(a_Args.Context(paramName)) builder.Add({Attrs.Parse}(type, a_Args));");
        }
        else
        {
          code.WriteLine("var constructor = type.GetConstructor( BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(GTMH.S11n.IGTInitArgs) }, null);");
          code.WriteLine($"if (constructor==null) throw new S11nException($\"Type '{{this.{this.Name}}}' has no suitable constructor\");");
          code.WriteLine($"using(a_Args.Context(paramName)) builder.Add(({InterfaceType})constructor.Invoke(new object[] {{ a_Args }}));");
        }
      }
      code.WriteLine("}");
    }
  }
}
