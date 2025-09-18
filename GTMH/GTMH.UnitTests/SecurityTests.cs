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
      await Assert.That(c.ToString()).StartsWith(Cipher.Pream);
      await Assert.That(c.ToString()).DoesNotContain("content");
      await Assert.That(c.ToString().Length).IsGreaterThan(Cipher.Pream.Length+"content".Length);

      await Assert.That(c.Decrypt(Secret)).IsEqualTo("content");

      var parsed = Cipher.TryParse(c.ToString(), out var _c);
      await Assert.That(parsed).IsTrue();
      await Assert.That(_c.Decrypt(Secret)).IsEqualTo("content");

      // salt should tumble content
      var d = Cipher.Encrypt("content", Secret);
      // same encrypted
      await Assert.That(d.Decrypt(Secret)).IsEqualTo("content");
      await Assert.That(d.ToString()).IsNotEqualTo(c.ToString());
    }
  }
}
