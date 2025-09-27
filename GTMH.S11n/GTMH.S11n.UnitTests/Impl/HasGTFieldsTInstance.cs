using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public interface IInstanceType
  {
    public string StringValue { get; }
  }
  public partial class InstanceType : IInstanceType
  {
    [GTField]
    public string StringValue { get; set; }
    public InstanceType(string a_Value)
    {
      StringValue = a_Value;
    }
  }

  public partial class HasGTFieldsTInstance
  {
    [GTField(GTInstance=true)]
    public readonly string Instance1;
    public IInstanceType Instance1Instance;
    public HasGTFieldsTInstance(string a_Instance1Value)
    {
      Instance1=nameof(InstanceType);
      Instance1Instance=new InstanceType(a_Instance1Value);
    }
  }
}
