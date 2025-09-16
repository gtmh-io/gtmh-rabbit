using System;
using System.Collections.Generic;
using System.Text;

namespace Tofye.IMQ
{
  public class PlainText : IDecryptor
  {
    public string Decrypt(string a_PlainText) => a_PlainText;
  }
}
