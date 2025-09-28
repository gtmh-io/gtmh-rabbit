using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public class GTInitArgs : IGTInitArgs
  {
    public static readonly string NoValue = Guid.NewGuid().ToString();
    private readonly IConfigProvider m_Provider;
    public GTInitArgs(IConfigProvider a_Config)
    {
      m_Provider = a_Config;
    }
    Stack<string> m_Context = new Stack<string>();
    string m_Prefix = "";

    public string GetValue(string a_Key, string a_Default)
    {
      var key = m_Prefix == "" ? a_Key : $"{m_Prefix}.{a_Key}";
      return m_Provider.GetValue(key, a_Default);
    }
    struct Context_t : IDisposable
    {
      public Action OnDipose;
      public void Dispose()
      {
        OnDipose();
      }
    }
    public IDisposable Context(string a_Name)
    {
      m_Context.Push(a_Name);
      m_Prefix=string.Join(".", m_Context);
      return new Context_t { OnDipose = () =>
      {
        m_Context.Pop();
        m_Prefix=string.Join(".", m_Context);
      } };
    }

    public Type ResolveType(string a_StringValue)
    {
      var rval = Type.GetType(a_StringValue);
      if ( rval != null ) return rval;
      foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        rval = assembly.GetType(a_StringValue);
        if(rval != null) return rval;
      }
      return null;
    }
  }
}
