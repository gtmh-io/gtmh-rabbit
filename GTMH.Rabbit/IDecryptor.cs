using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit
{
  public interface IDecryptor
  {
    string Decrypt(string a_Encrypted);
  }
}
