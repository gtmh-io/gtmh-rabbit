using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsTInstanceArrayRequired
  {
    /// <summary>
    /// Setting Required to be true allows us to specify non-nullable instance types
    /// </summary>
    [GTS11n(Instance=true, Required=true)]
    public ImmutableArray<Interface_t> Instances { get; }
    public HasGTFieldsTInstanceArrayRequired(params Interface_t[] a_Instances)
    {
      Instances = ImmutableArray.Create(a_Instances);
    }
  }
}
