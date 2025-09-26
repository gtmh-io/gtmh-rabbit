using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public class DictionaryConfig : IConfigProvider, IEnumerable<KeyValuePair<string, string>> 
  {
    private Dictionary<string,string> m_Data = new Dictionary<string,string>();
    public DictionaryConfig() { }
    public DictionaryConfig(IEnumerable<KeyValuePair<string, string>> data)
    {
      foreach(var kvp in data)
      {
        m_Data.Add(kvp.Key, kvp.Value);
      }
    }
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)m_Data).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Data).GetEnumerator();
    public void Add(string a_Key, string a_Value)
    {
      m_Data.Add(a_Key, a_Value);
    }

    public string GetValue(string a_Key, string a_Default)
    {
      if ( ! m_Data.TryGetValue(a_Key, out string rval)) return a_Default;
      else return rval;
    }
  }
}
