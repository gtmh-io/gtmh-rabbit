using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public interface IGTInitArgs
  {
    string GetValue(string a_Key, string a_Default);
    IDisposable Context(string a_Name);
    Type ResolveType(string a_StringValue);
  }
}
