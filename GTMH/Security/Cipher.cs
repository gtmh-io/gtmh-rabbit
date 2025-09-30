using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GTMH.Security;

// based off own code, leaning on claude. sorry not sorry

public sealed class Cipher
{
  private const int SaltSize = 32; // 256 bits
  private const int KeySize = 32;  // 256 bits
  private const int NonceSize = 12; // 96 bits for AES-GCM (standard)
  private const int TagSize = 16;  // 128 bits for GCM
  private const int Iterations = 100000; // PBKDF2 iterations
  private const int MaxPayloadSize = 1 << 30; // 1 GB

  // Version for future compatibility
  private const byte CurrentVersion = 1;
  private const string HeaderPrefix = "GTMH";
  private const int HeaderPrefixLength = 4;

  public ReadOnlyMemory<byte> EncryptedData { get; }

  private Cipher(byte[] encryptedData)
  {
    EncryptedData = encryptedData;
  }

  /// <summary>
  /// Encrypts data using AES-GCM with PBKDF2 key derivation
  /// </summary>
  public static Cipher Encrypt(string plainText, string password)
  {
    if(string.IsNullOrEmpty(plainText))
      throw new ArgumentNullException(nameof(plainText));
    if(string.IsNullOrEmpty(password))
      throw new ArgumentNullException(nameof(password));

    var plainBytes = Encoding.UTF8.GetBytes(plainText);
    return Encrypt(plainBytes, password);
  }

  public static Cipher Encrypt(byte[] plainBytes, string password)
  {
    if(plainBytes == null || plainBytes.Length == 0)
      throw new ArgumentNullException(nameof(plainBytes));
    if(string.IsNullOrEmpty(password))
      throw new ArgumentNullException(nameof(password));
    if(plainBytes.Length > MaxPayloadSize)
      throw new ArgumentException($"Data too large (max {MaxPayloadSize} bytes)");

    // Generate cryptographically secure random salt
    var salt = RandomNumberGenerator.GetBytes(SaltSize);

    // Derive key using PBKDF2
    var key = Rfc2898DeriveBytes.Pbkdf2(
        password: password,
        salt: salt,
        iterations: Iterations,
        hashAlgorithm: HashAlgorithmName.SHA256,
        outputLength: KeySize);

    // Generate random nonce (12 bytes for AES-GCM)
    var nonce = RandomNumberGenerator.GetBytes(NonceSize);

    // Encrypt using AES-GCM
    using(var aes = new AesGcm(key, TagSize))
    {
      var cipherText = new byte[plainBytes.Length];
      var tag = new byte[TagSize];

      aes.Encrypt(nonce, plainBytes, cipherText, tag);

      // Combine all components
      // Format: [version(1)][salt(32)][nonce(12)][tag(16)][ciphertext]
      var result = new byte[1 + SaltSize + NonceSize + TagSize + cipherText.Length];

      result[0] = CurrentVersion;
      Buffer.BlockCopy(salt, 0, result, 1, SaltSize);
      Buffer.BlockCopy(nonce, 0, result, 1 + SaltSize, NonceSize);
      Buffer.BlockCopy(tag, 0, result, 1 + SaltSize + NonceSize, TagSize);
      Buffer.BlockCopy(cipherText, 0, result, 1 + SaltSize + NonceSize + TagSize, cipherText.Length);

      return new Cipher(result);
    }
  }

  /// <summary>
  /// Decrypts data encrypted with Encrypt method
  /// </summary>
  public string DecryptString(string password)
  {
    var decrypted = Decrypt(password);
    return Encoding.UTF8.GetString(decrypted);
  }

  public byte[] Decrypt(string password)
  {
    if(string.IsNullOrEmpty(password))
      throw new ArgumentNullException(nameof(password));

    var encryptedBytes = EncryptedData.ToArray();

    if(encryptedBytes.Length < 1 + SaltSize + NonceSize + TagSize)
      throw new ArgumentException("Invalid encrypted data");

    // Check version
    var version = encryptedBytes[0];
    if(version != CurrentVersion)
      throw new NotSupportedException($"Unsupported cipher version: {version}");

    // Extract components
    var salt = new byte[SaltSize];
    var nonce = new byte[NonceSize];
    var tag = new byte[TagSize];

    Buffer.BlockCopy(encryptedBytes, 1, salt, 0, SaltSize);
    Buffer.BlockCopy(encryptedBytes, 1 + SaltSize, nonce, 0, NonceSize);
    Buffer.BlockCopy(encryptedBytes, 1 + SaltSize + NonceSize, tag, 0, TagSize);

    var cipherTextLength = encryptedBytes.Length - 1 - SaltSize - NonceSize - TagSize;
    var cipherText = new byte[cipherTextLength];
    Buffer.BlockCopy(encryptedBytes, 1 + SaltSize + NonceSize + TagSize, cipherText, 0, cipherTextLength);

    // Derive key
    var key = Rfc2898DeriveBytes.Pbkdf2(
        password: password,
        salt: salt,
        iterations: Iterations,
        hashAlgorithm: HashAlgorithmName.SHA256,
        outputLength: KeySize);

    // Decrypt
    using(var aes = new AesGcm(key, TagSize))
    {
      var plainText = new byte[cipherTextLength];

      try
      {
        aes.Decrypt(nonce, cipherText, tag, plainText);
        return plainText;
      }
      catch(CryptographicException)
      {
        throw new InvalidOperationException("Decryption failed - invalid password or corrupted data");
      }
    }
  }

  /// <summary>
  /// Converts the cipher to a string representation
  /// Format: "GTMHSC:v1:base64data"
  /// </summary>
  public override string ToString()
  {
    var base64 = Convert.ToBase64String(EncryptedData.Span);
    return $"{HeaderPrefix}:v{CurrentVersion}:{base64}";
  }

  /// <summary>
  /// Tries to parse a string representation back to Cipher
  /// </summary>
  public static bool TryParse(string value, out Cipher? cipher)
  {
    cipher = null;

    if(string.IsNullOrWhiteSpace(value))
      return false;

    var parts = value.Split(':', 3);
    if(parts.Length != 3)
      return false;

    // Check header
    if(parts[0] != HeaderPrefix)
      return false;

    // Check version
    if(!parts[1].StartsWith("v") || !byte.TryParse(parts[1].Substring(1), out var version))
      return false;

    // Currently only support version 1
    if(version != CurrentVersion)
      return false;

    // Parse base64 data
    try
    {
      var encryptedData = Convert.FromBase64String(parts[2]);

      // Validate minimum length
      if(encryptedData.Length < 1 + SaltSize + NonceSize + TagSize)
        return false;

      // Validate version byte in data matches
      if(encryptedData[0] != version)
        return false;

      cipher = new Cipher(encryptedData);
      return true;
    }
    catch(FormatException)
    {
      return false;
    }
  }

  /// <summary>
  /// Parses a string representation to Cipher
  /// </summary>
  public static Cipher Parse(string value)
  {
    if(TryParse(value, out var cipher) && cipher != null)
      return cipher;

    throw new FormatException("Invalid Cipher string format");
  }
  
}