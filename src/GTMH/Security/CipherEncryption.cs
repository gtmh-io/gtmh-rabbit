using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Security
{
  public class CipherEncryption : IDecryptor
  {
    private readonly string Secret;

    public CipherEncryption(string a_Secret)
    {
      this.Secret = a_Secret;
    }
    public CipherEncryption(ISecretProvider a_Provider)
    {
      this.Secret = a_Provider.Secret;
    }
    public string Decrypt(string a_Encrypted)
    {
      if(!Cipher.TryParse(a_Encrypted, out var cipher))
      {
        throw new ArgumentException("Your data does not appear to be cipher encrypted");
      }
#pragma warning disable 8602 // possible null
      return cipher.DecryptString(Secret);
#pragma warning restore 8602
    }
    public string Encrypt(string a_PlainText) => Cipher.Encrypt(a_PlainText, Secret).ToString();
  }
}
