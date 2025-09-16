using System;
using System.Collections.Generic;
using System.Text;

namespace Tofye.IMQ
{
  public interface IDecryptor
  {
    string Decrypt(string a_Encrypted);
  }
}
