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
    }

    public void WriteInitialisation(Code code)
    {
    }
  }
}
