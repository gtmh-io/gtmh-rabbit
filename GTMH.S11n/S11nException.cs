using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n
{
  public class S11nException : Exception
  {
    public S11nException(string a_Err) : base(a_Err) { }  
  }
}
