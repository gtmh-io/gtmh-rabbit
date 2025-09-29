using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace GTMH.S11n.UnitTests.Impl
{
  public partial class HasGTFieldsTInstanceArray
  {
    [GTS11n(Instance=true)]
    public ImmutableArray<Interface_t?> Instances { get; }
    public HasGTFieldsTInstanceArray(params Interface_t?[] a_Instances)
    {
      Instances = ImmutableArray.Create(a_Instances);
    }
  }
  public partial class HasGTFieldsTInstanceArrayAKA
  {
    [GTS11n(Instance=true, AKA="OldInstances")]
    public ImmutableArray<Interface_t?> Instances { get; }
    public HasGTFieldsTInstanceArrayAKA(params Interface_t?[] a_Instances)
    {
      Instances = ImmutableArray.Create(a_Instances);
    }
  }
  public partial class HasGTFieldsTInstanceArrayCustomS11n
  {
    [GTS11n(Instance=true, Parse=nameof(Parse), DeParse=nameof(DeParse))]
    public ImmutableArray<Interface_t?> Instances { get; }
    public HasGTFieldsTInstanceArrayCustomS11n(params Interface_t?[] a_Instances)
    {
      Instances = ImmutableArray.Create(a_Instances);
    }
    static Interface_t Parse(Type a_Type, IGTInitArgs a_Args)
    {
      // just a copy+paste from what would be generated
      var constructor = a_Type.GetConstructor( BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(GTMH.S11n.IGTInitArgs)}, null);
      if ( constructor == null ) throw new Exception("Herve is an idiot");
      return (Interface_t)constructor.Invoke(new object[] { a_Args });

    }
    static void DeParse(string a_Name, Interface_t ? a_Resource, IGTParseArgs a_Args)
    {
      // just a copy+paste from what would be generated
      a_Args.Add(a_Name, a_Args.DisolveType(a_Resource));
      using(a_Args.Context(a_Name)) a_Args.S11nGather(a_Resource);
    }
  }
  public partial class HasGTFieldsTInstanceArrayCustomS11nAKA
  {
    [GTS11n(Instance=true, Parse=nameof(Parse), DeParse=nameof(DeParse), AKA ="OldInstances")]
    public ImmutableArray<Interface_t?> Instances { get; }
    public HasGTFieldsTInstanceArrayCustomS11nAKA(params Interface_t?[] a_Instances)
    {
      Instances = ImmutableArray.Create(a_Instances);
    }
    static Interface_t Parse(Type a_Type, IGTInitArgs a_Args)
    {
      // just a copy+paste from what would be generated
      var constructor = a_Type.GetConstructor( BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(GTMH.S11n.IGTInitArgs)}, null);
      if ( constructor == null ) throw new Exception("Herve is an idiot");
      return (Interface_t)constructor.Invoke(new object[] { a_Args });

    }
    static void DeParse(string a_Name, Interface_t ? a_Resource, IGTParseArgs a_Args)
    {
      // just a copy+paste from what would be generated
      a_Args.Add(a_Name, a_Args.DisolveType(a_Resource));
      using(a_Args.Context(a_Name)) a_Args.S11nGather(a_Resource);
    }
  }
}
