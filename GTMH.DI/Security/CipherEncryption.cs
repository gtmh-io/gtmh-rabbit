using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GTMH.DI.Security
{
  public class CipherConfig
  {
    [Required]
    public required string Secret { get; set; }
    public static Dictionary<string, string> GetCommandLineMappings()
    {
      var rval = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      rval.Add("-gtmh.secret", $"{nameof(CipherConfig)}:{nameof(Secret)}");
      rval.Add("--gtmh.secret", $"{nameof(CipherConfig)}:{nameof(Secret)}");
      return rval;
    }
  }
  public class CipherEncryption : GTMH.Security.IDecryptor
  {
    private readonly GTMH.Security.CipherEncryption m_Impl;
    public CipherEncryption(IOptions<CipherConfig> config)
    {
      m_Impl = new GTMH.Security.CipherEncryption(config.Value.Secret);
    }
    public string Decrypt(string a_Encrypted) => m_Impl.Decrypt(a_Encrypted);
  }
}
