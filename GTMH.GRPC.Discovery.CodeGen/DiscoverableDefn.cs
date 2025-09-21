using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GTMH.GRPC.Discovery.CodeGen
{
  internal struct DiscoverableDefn
  {
    public readonly string Visibility;
    public readonly string DiscoverableClass;
    public readonly string DiscoverableType;
    public readonly string Namespace;
    public string ClientMethodName => "Locate";
    public readonly List<string> Usings;

    public DiscoverableDefn(string a_Visibility, string a_NS, string a_DiscoverableClass, string a_DiscoverableType, List<string> a_Usings)
    {
      this.Visibility = a_Visibility;
      Namespace = a_NS;
      this.DiscoverableClass = a_DiscoverableClass;
      this.DiscoverableType = a_DiscoverableType;
      this.Usings = a_Usings;
    }
  }
}
