using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public interface IGTParseArgs
  {
    Dictionary<string, string> Value { get; }
    void Add(string a_Key, string a_Value);
    IDisposable Context(string a_Name);
    void S11nGather(dynamic a_Instance);
    string DisolveType(object a_Value);
  }
}
