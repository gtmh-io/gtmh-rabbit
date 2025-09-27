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
      //code.WriteLine("TODO");
    }

    public void WriteInitialisation(Code code)
    {
      //code.WriteLine("TODO");
    }
  }
}
