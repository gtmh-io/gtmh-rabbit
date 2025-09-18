using System;
using System.Collections.Generic;
using System.Text;

using GTMH.Security;

namespace GTMH.UnitTests
{
  public class SecurityTests
  {
    public readonly string Secret = Guid.NewGuid().ToString();
    [Test]
    public async ValueTask TestCipher()
    {
      var c = Cipher.Encrypt("content", Secret);
      await Assert.That(c.ToString()).DoesNotContain("content");

      await Assert.That(c.DecryptString(Secret)).IsEqualTo("content");

      var parsed = Cipher.TryParse(c.ToString(), out var _c);
      await Assert.That(parsed).IsTrue();
#pragma warning disable 8602 // possible null
      await Assert.That(_c.DecryptString(Secret)).IsEqualTo("content");
#pragma warning restore 8602 // possible null

      // salt should tumble content
      var d = Cipher.Encrypt("content", Secret);
      // same encrypted
      await Assert.That(d.DecryptString(Secret)).IsEqualTo("content");
      // but not same representation
      await Assert.That(d.ToString()).IsNotEqualTo(c.ToString());
    }
    [Test]
    public async ValueTask CipherEncryption()
    {
      var e = new CipherEncryption("password");
      var encrypted = e.Encrypt("plain text");
      var decrypted = e.Decrypt(encrypted);
      await Assert.That(decrypted).IsEqualTo("plain text");

      var str="GTMH:v1:AcMBYbSjcaHXxMoDXChEq+Q/eMOVDPA9MXRDwV5IaH9oDmDTQsNkiXeELvzXQ2oVQ1a44vx2/mDc4yD5WIyDNq/Sbio+/xs=";
      var _decrypted=e.Decrypt(str);
      await Assert.That(_decrypted).IsEqualTo("plain text");

      str="GTMH:v1:AU0OfGgnohfk73hyf5K1k0ApnOVoj0sOn0sr2PMsFj/KC4gUS9RvKweK/NG1v/1DtWOSAoBCfwahx7tDeeHO/dw=";
      _decrypted=e.Decrypt(str);
      await Assert.That(_decrypted).IsEqualTo("text");
    }
  }
}
