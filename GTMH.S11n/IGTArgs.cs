using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public interface IGTArgs
  {
    string GetValue(string a_Key, string a_Default);
  }
}
