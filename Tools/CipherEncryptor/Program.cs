using GTMH.Security;
using GTMH.Util;

Func<int> PrintUsage=()=>
{
  Console.WriteLine("Usage:");
  Console.WriteLine("Optional: --secret=<secret>");
  Console.WriteLine("Required: --value=<value>");
  Console.WriteLine("Required: --action=<action>");
  Console.WriteLine("where <action> must be encrypt or decrypt");
  return -1;
};

var secret = args.GetCmdLine("secret");

var value = args.GetCmdLine("value");
if(value == null) return PrintUsage();

if(secret == null)
{
  Console.WriteLine("Enter secret");
  secret=Console.ReadLine();
  if ( secret == null) return PrintUsage();
}

var ce=new CipherEncryption(secret);

switch(args.GetCmdLine("action", "unspecified").ToLower())
{
  case "encrypt":
  {
    var encrypted=ce.Encrypt(value);
    Console.WriteLine(encrypted);
    return 0;
  }
  case "decrypt":
  {
    try
    {
      var decrypted = ce.Decrypt(value);
      Console.WriteLine(decrypted);
      return 0;
    }
    catch(FormatException)
    {
      Console.Error.WriteLine("Your value does not appear to be properly encrypted");
      return -2;
    }
  }
  default:
  {
    return PrintUsage();
  }
}
