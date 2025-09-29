using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace GTMH.S11n.UnitTests.Impl
{
  public interface IHaveNonS8bleResource
  {
    public string S8ble { get; }
    public Guid NonS8ble { get; }
  }
  public partial class HaveNonS8bleResource : IHaveNonS8bleResource
  {
    [GTS11n]
    public string S8ble { get; private set;}
    public Guid NonS8ble { get; }
    public HaveNonS8bleResource(string a_string, Guid a_Guid)
    {
      S8ble=a_string;
      NonS8ble=a_Guid;
    }
    [GTS11nInit]
    public HaveNonS8bleResource(IGTInitArgs a_Args, Guid a_Guid)
    {
      this.SetS11n(a_Args);
      NonS8ble=a_Guid;
    }
  }

  public partial class HasGTFieldsTInstanceNonS8bleResource
  {
    [GTS11n(Instance =true, Parse=nameof(Parse), DeParse=nameof(DeParse), AKA="OldInterface")]
    public readonly IHaveNonS8bleResource ? Interface;

    public HasGTFieldsTInstanceNonS8bleResource(string a_StringValue)
    {
      this.Interface = new HaveNonS8bleResource(a_StringValue, CreateResource());
    }
    public static Guid CreateResource()=>Guid.NewGuid();
    static IHaveNonS8bleResource Parse(Type a_Type, IGTInitArgs a_Args)
    {
      var constructor = a_Type.GetConstructor( BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(GTMH.S11n.IGTInitArgs), typeof(Guid) }, null);
      if ( constructor == null ) throw new Exception("Herve is an idiot");
      return (IHaveNonS8bleResource)constructor.Invoke(new object[] { a_Args, CreateResource() });
    }
    // can't think of a use case but implemented for compeleteness aping generated code
    static void DeParse(string a_Name, IHaveNonS8bleResource a_Resource, IGTParseArgs a_Args)
    {
      a_Args.Add(a_Name, a_Args.DisolveType(a_Resource));
      using(a_Args.Context(a_Name)) a_Args.S11nGather(a_Resource);
    }
  }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS8601 // Possible null reference assignment.
