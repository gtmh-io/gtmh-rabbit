using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public static class S11nXM
  {
    public static IGTInitArgs ForInit(this IConfigProvider a_Config)
    {
      if ( a_Config == null) throw new ArgumentException("Require non-null config");
      else return new GTInitArgs(a_Config);
    }
  }
}
