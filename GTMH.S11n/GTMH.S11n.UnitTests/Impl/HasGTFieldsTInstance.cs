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
#pragma warning disable CS8618
    public HasGTFieldsTInstance(string a_Instance1Value)
    {
#pragma warning disable CS8601// Possible null reference assignment.
      Instance1 =typeof(InstanceType).FullName;
#pragma warning restore CS8601// Possible null reference assignment.
      Instance1Instance =new InstanceType(a_Instance1Value);
    }
#pragma warning restore CS8618
  }
}
