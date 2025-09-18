using GTMH.Security;
using GTMH.Util;

Func<int> PrintUsage=()=>
{
  Console.WriteLine("Usage:");
  Console.WriteLine("Optinal: --secret=<secret>");
  Console.WriteLine("Required: --value=<value>");
  Console.WriteLine("Required: --action=<action>");
  Console.WriteLine("where <action> must be encrypt or decrypt");
  return -1;
};

var secret = args.GetCmdLine("secret");
if(secret == null) return PrintUsage();

var value = args.GetCmdLine("value");
if(value == null) return PrintUsage();

var ce=new CipherEncryption(secret);

switch(args.GetCmdLine("action", "unspecified").ToLower())
{
  case "encrypt":
  {
    var encrypted=ce.Encrypt(value);
    var decrypted=ce.Decrypt(encrypted);
    Console.WriteLine(encrypted);
    return 0;
  }
  case "decrypt":
  {
    var decrypted=ce.Decrypt(value);
    Console.WriteLine(decrypted);
    return 0;
  }
  default:
  {
    return PrintUsage();
  }
}
