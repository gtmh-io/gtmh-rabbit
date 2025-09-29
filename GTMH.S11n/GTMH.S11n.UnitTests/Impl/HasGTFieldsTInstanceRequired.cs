using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsTInstanceRequired
  {
    [GTField(GTInit=true, Required=true)]
    public readonly Interface_t Required;
    [GTField(GTInit=true)]
    public readonly Interface_t ? Optional;
    public HasGTFieldsTInstanceRequired(Interface_t a_Required, Interface_t? a_Optional = null)
    {
      this.Required = a_Required;
      this.Optional = a_Optional;
    }
  }
}
