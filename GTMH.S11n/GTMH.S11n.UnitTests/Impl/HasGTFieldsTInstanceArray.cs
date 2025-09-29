using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsTInstanceArray
  {
    [GTField(GTInit=true)]
    public ImmutableArray<Interface_t> Instances { get; }
    public HasGTFieldsTInstanceArray(params Interface_t[] a_Instances)
    {
      Instances = ImmutableArray.Create(a_Instances);
    }
  }
}
