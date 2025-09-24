using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public class Code
  {
    private StringBuilder m_Code = new StringBuilder();
    private int m_Indent = 0;
    public override string ToString() => m_Code.ToString();
    public void WriteLine(string a_Statement)
    {
      for(var i = 0; i != m_Indent; i++)
      {
        m_Code.Append("  ");
      }
      m_Code.AppendLine(a_Statement);
    }
    public IDisposable Indent()
    {
      m_Indent += 1;
      return new Indent_t(()=> m_Indent-=1 );
    }
    struct Indent_t : IDisposable
    {
      private Action DecreaseIndent;

      public Indent_t(Action a_DecreaseIndent)
      {
        DecreaseIndent = a_DecreaseIndent;
      }
      public void Dispose()
      {
        DecreaseIndent();
      }
    }
  }
}
