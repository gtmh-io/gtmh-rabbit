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
    [GTS11n(AKA="OldStringProperty")]
    public string NewStringProperty { get; set; }
    public InstanceType(string a_Value)
    {
      NewStringProperty = a_Value;
    }
  }

  public partial class HasGTFieldsTInstance
  {
    [GTS11n(Instance=true, AKA ="OldInstance")]
    public IInstanceType ?NewInstance;
    [GTS11n(Instance=true)]
    public IInstanceType ?OtherInstance;
    public HasGTFieldsTInstance(string a_NewInstanceValue, string a_OtherInstanceValue)
    {
      NewInstance =new InstanceType(a_NewInstanceValue);
      OtherInstance = new InstanceType(a_OtherInstanceValue);
    }
  }
}
