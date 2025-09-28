using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public interface IInstanceType
  {
    public string NewStringProperty { get; }
  }
  public partial class InstanceType : IInstanceType
  {
    [GTField(AKA="OldStringProperty")]
    public string NewStringProperty { get; set; }
    public InstanceType(string a_Value)
    {
      NewStringProperty = a_Value;
    }
  }

  public partial class HasGTFieldsTInstance
  {
    [GTField(GTInstance=true, AKA ="OldInstance")]
    public readonly string NewInstance;
    public IInstanceType NewInstanceInstance;
    [GTField(GTInstance=true)]
    public readonly string OtherInstance;
    public IInstanceType OtherInstanceInstance;
#pragma warning disable CS8618
    public HasGTFieldsTInstance(string a_NewInstanceValue, string a_OtherInstanceValue)
    {
#pragma warning disable CS8601// Possible null reference assignment.
      NewInstance =typeof(InstanceType).FullName;
      OtherInstance = typeof(InstanceType).FullName;
#pragma warning restore CS8601// Possible null reference assignment.
      NewInstanceInstance =new InstanceType(a_NewInstanceValue);
      OtherInstanceInstance = new InstanceType(a_OtherInstanceValue);
    }
#pragma warning restore CS8618
  }
}
