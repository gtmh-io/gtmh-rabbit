using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public class GTArgs : IGTArgs
  {
    public static readonly string NoValue = Guid.NewGuid().ToString();
    private readonly IConfigProvider m_Provider;
    public GTArgs(IConfigProvider a_Config)
    {
      m_Provider = a_Config;
    }

    public string GetValue(string a_Key, string a_Default)
    {
      return m_Provider.GetValue(a_Key, a_Default);
    }
  }
}
