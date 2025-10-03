using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Security
{
  public class EnvSecretProvider : ISecretProvider
  {
    public string Secret { get; }
    public EnvSecretProvider(string a_VarName)
    {
      var value = Environment.GetEnvironmentVariable(a_VarName);
      if ( value == null ) throw new Exception($"Could find no environment varialbe for '{a_VarName}'");
      Secret = value;
    }
  }
}
