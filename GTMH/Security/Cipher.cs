using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;


namespace GTMH.Security
{
  public struct Cipher
  {
		internal Cipher(string a_Value, string a_Salt) : this(System.Convert.FromBase64String(a_Value), System.Convert.FromBase64String(a_Salt)) { }
    internal Cipher(byte[] a_Value, byte[] a_Salt)
		{
			Value = a_Value;
			Salt = a_Salt;
			if (this.Value.Length > MaxPayload) throw new Exception("Too much data - use a different method to encrypt");
		}
    public readonly ReadOnlyMemory<byte> Value;
    public string ValueStr { get { return System.Convert.ToBase64String(Value.Span); } }
    public readonly byte[] Salt;
    public string SaltStr { get { return System.Convert.ToBase64String(Salt); } }

    public static Cipher Encrypt(string a_PlainText, string a_AppSecret)
    {
      using (PasswordDeriveBytes secret = new PasswordDeriveBytes(a_AppSecret, null))
      using (var algo = Aes.Create())
      {
        algo.Mode = CipherMode.CBC;
        var salt = NewSaltBytes();
        using (ICryptoTransform encryptor = algo.CreateEncryptor(secret.GetBytes(algo.KeySize / 8), salt))
        using (MemoryStream memoryStream = new MemoryStream())
        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
          byte[] plainTextBytes = Encoding.UTF8.GetBytes(a_PlainText);
          cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
          cryptoStream.FlushFinalBlock();
          byte[] cipherTextBytes = memoryStream.ToArray();
          return new Cipher(cipherTextBytes, salt);
        }
      }
    }
		public static Cipher Encrypt(byte [] a_Bytes, string a_AppSecret)
		{
      using (PasswordDeriveBytes secret = new PasswordDeriveBytes(a_AppSecret, null))
      using (var algo = Aes.Create())
      {
        algo.Mode = CipherMode.CBC;
        var salt = NewSaltBytes();
        using (ICryptoTransform encryptor = algo.CreateEncryptor(secret.GetBytes(algo.KeySize / 8), salt))
        using (MemoryStream memoryStream = new MemoryStream())
        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
          cryptoStream.Write(a_Bytes, 0, a_Bytes.Length);
          cryptoStream.FlushFinalBlock();
          byte[] cipherTextBytes = memoryStream.ToArray();
          return new Cipher(cipherTextBytes, salt);
        }
      }
		}

    public string Decrypt( string a_AppSecret)
    {
      using (PasswordDeriveBytes phrase = new PasswordDeriveBytes(a_AppSecret, null))
      using (var algo = Aes.Create())
      {
        algo.Mode = CipherMode.CBC;
        byte[] keyBytes = phrase.GetBytes(algo.KeySize / 8);
        using (ICryptoTransform decryptor = algo.CreateDecryptor(keyBytes, this.Salt))
        using(MemoryStream memoryStream = new MemoryStream(this.Value.ToArray()))
        using(CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        {
          byte[] plainTextBytes = new byte[this.Value.Length];
          int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
          return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
      }
    }

    public byte[] DecryptBytes( string a_AppSecret)
    {
      using (PasswordDeriveBytes phrase = new PasswordDeriveBytes(a_AppSecret, null))
      using (var algo = Aes.Create())
      {
        algo.Mode = CipherMode.CBC;
        byte[] keyBytes = phrase.GetBytes(algo.KeySize / 8);
        using (ICryptoTransform decryptor = algo.CreateDecryptor(keyBytes, this.Salt))
        using(MemoryStream memoryStream = new MemoryStream(this.Value.ToArray()))
        using(CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        {
          byte[] plainTextBytes = new byte[this.Value.Length];
          int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
          var rval = new byte[decryptedByteCount];
          Array.Copy(plainTextBytes, rval, decryptedByteCount);
          return rval;
        }
      }
    }

    private static byte[] NewSaltBytes()
    {
      var rval = new byte[SaltBytes];
      (new Random((unchecked((int)DateTime.Now.Ticks)))).NextBytes(rval);
      return rval;
    }

    public const string Pream = "GTMHCIPHER";
    const int LW = 3;
		public const int SaltBytes = 16;
		public const int MaxPayload = 1 << 30; // 1 GB
    public override string ToString()
    {
			var slt = this.SaltStr;
      if (slt.Length > 999) throw new Exception("Herve is dumb");
      return String.Format("{0}{1}{2}{3}", Pream, slt.Length.ToString().PadLeft(LW, '0'), slt, this.ValueStr);
    }

    public static Cipher Parse(string a_Value)
    {
      Cipher rval;
      if (!TryParse(a_Value, out rval))
      {
        throw new ArgumentException("Failed parse cipher");
      }
      return rval;
    }
    public static bool TryParse(string a_Value, out Cipher a_Rval)
    {
      if (a_Value == null || a_Value.Length < Pream.Length + LW)
      {
        a_Rval = new Cipher();
        return false;
      }

      int sw;

      if (!int.TryParse(a_Value.Substring(Pream.Length, LW), out sw) || sw<1)
      {
        a_Rval = new Cipher();
        return false;
      }
			a_Value = a_Value.Trim();
			if ( a_Value.Length < Pream.Length + LW+sw )
      {
        a_Rval = new Cipher();
        return false;
      }

      try
      {
        a_Rval = new Cipher(a_Value.Substring(Pream.Length + LW + sw), a_Value.Substring(Pream.Length + LW, sw));
      }
      catch (FormatException)
      {
        a_Rval = new Cipher();
        return false;
      }
      return true;
    }

		public byte[] ToBytes()
		{
			using (var mem = new System.IO.MemoryStream())
			using (var writer = new System.IO.BinaryWriter(mem))
			{
				writer.Write(System.Convert.FromBase64String(Pream));
				writer.Write(Salt);
				writer.Write(this.Value.Length);
				writer.Write(this.Value.ToArray());
				return mem.ToArray();
			}
		}

		private static bool fail(out Cipher a_Rval)
		{
			a_Rval = new Cipher();
			return false;
		}
		public static bool TryParse(byte[] a_Value, out Cipher a_Rval)
		{
			if (a_Value == null || a_Value.Length == 0)
			{
				return fail(out a_Rval);
			}
			var preamBytes = System.Convert.FromBase64String(Pream);
			byte[] buf = new byte[0];
			Func<byte[], bool> cmp_buf = expect =>
			{
				if (expect == null || expect.Length != buf.Length) return false;
				for (int i = 0; i != expect.Length; ++i)
				{
					if (buf[i] != expect[i]) return false;
				}
				return true;
			};
			using (var mem = new System.IO.MemoryStream(a_Value))
			using (var reader = new BinaryReader(mem))
			{
				buf = reader.ReadBytes(preamBytes.Length);
				if (!cmp_buf(preamBytes)) return fail(out a_Rval);
				var salt = reader.ReadBytes(SaltBytes);
				if (salt == null || salt.Length != SaltBytes) return fail(out a_Rval);

				var payloadLength = reader.ReadInt32();
				if (payloadLength < 0 || payloadLength > MaxPayload) return fail(out a_Rval);
				var payload = reader.ReadBytes(payloadLength);
				if (payload == null || payload.Length != payloadLength) return fail(out a_Rval);

				a_Rval = new Cipher(payload, salt);
				return true;
			}
		}
  }
}
