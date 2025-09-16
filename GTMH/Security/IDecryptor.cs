using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Security
{
  public interface IDecryptor
  {
    string Decrypt(string a_Encrypted);
  }
}
