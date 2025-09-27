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
    public InstanceField(string a_Name, string backingName, string a_InterfaceType)
    {
      Name = a_Name;
      InstanceMemberName = backingName;
      InterfaceType = a_InterfaceType;
    }

    public void WriteGather(Code code, string a_Args)
    {
      code.WriteLine($"// TODO Name={Name} InstanceMemberName={InstanceMemberName} InterfaceType={InterfaceType}");
      code.WriteLine($"a_Args.Add(\"{Name}\", {InstanceMemberName}.GetType().FullName);");
      code.WriteLine($"using(a_Args.Context(\"{Name}\")) a_Args.S11nGather(this.{InstanceMemberName});");
    }

    public void WriteInitialisation(Code code)
    {
      code.WriteLine($"// TODO Name={Name} InstanceMemberName={InstanceMemberName} InterfaceType={InterfaceType}");
    }
  }
}
