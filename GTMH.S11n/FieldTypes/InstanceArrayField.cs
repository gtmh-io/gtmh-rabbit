using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
  internal class InstanceArrayField : IFieldType
  {
    private readonly string Name;
    private readonly string InstanceType;
    private readonly GTFieldAttrs Attrs;

    public InstanceArrayField(string a_Name, string a_InstanceType, GTFieldAttrs a_Attrs)
    {
      this.Name = a_Name;
      this.InstanceType = a_InstanceType;
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
            code.WriteLine($"if ( this.{Name} !=null) {Attrs.DeParse}(\"{Name}\", this.{Name}, a_Args);");
          }
          else
          {
            code.WriteLine($"a_Args.Add($\"{Name}.{{idx}}\", a_Args.DisolveType(this.{Name}[idx]));");
            code.WriteLine($"using(a_Args.Context(\"{Name}.{{idx}}\")) if ( this.{Name}[idx] !=null) a_Args.S11nGather(this.{Name}[idx]);");
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
        code.WriteLine($"if ( tmp == GTInitArgs.NoValue ) throw new S11nException(\"'{Name}' marked as array but no length found\");");
        code.WriteLine($"int N;");
        code.WriteLine($"if ( ! int.TryParse(tmp, out N) || N <0 ) throw new S11nException(\"Invalid length={{N}} for '{Name}'\");");
        code.WriteLine("// TODO - do I want a sanity check on length");
        code.WriteLine($"var builder=System.Collections.Immutable.ImmutableArray.CreateBuilder<{InstanceType}>(N);");
      }
      code.WriteLine("}");
    }
  }
}
