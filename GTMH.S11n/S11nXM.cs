using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public static class S11nXM
  {
    public static IGTArgs ForInit(this IConfigProvider a_Config)
    {
      if ( a_Config == null) throw new ArgumentException("Require non-null config");
      else return new GTArgs(a_Config);
    }
  }
}
