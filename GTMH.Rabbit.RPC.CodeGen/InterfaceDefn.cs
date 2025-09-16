using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC.CodeGen
{
  public readonly struct MethodArg
  {
    public readonly string Type;
    public readonly string Name;
    public MethodArg(string a_Type, string a_Name)
    {
      Type=a_Type;
      Name = a_Name;
    }
  }
  public readonly struct MethodDefn
  {
    public readonly string Name;
    public readonly string Type;
    public readonly List<MethodArg> Args;
    public MethodDefn(string a_Type, string a_Name, List<MethodArg> a_Args)
    {
      Name =a_Name;
      Type = a_Type;
      Args = a_Args;
    }
  }

  public readonly struct InterfaceDefn
  {
    public readonly string Name;
    public readonly string Namespace;
    public readonly List<string> Usings;
    public readonly List<MethodDefn> Methods;
    public InterfaceDefn(string a_Name, string a_NS, List<string> a_Usings, List<MethodDefn> a_Methods)
    {
      Name = a_Name;
      Namespace = a_NS;
      Usings = a_Usings;
      Methods = a_Methods;
    }

    internal string GetImplClassName(string a_Suffix)
    {
      if(Name.StartsWith("I"))
      {
        return $"{Name.Substring(1)}{a_Suffix}";
      }
      else
      {
        return $"{Name}{a_Suffix}";
      }
    }
  }
}
