using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Util
{
  public static class CmdLineArgs
  {
    public static string GetCmdLine(this string[] args, string a_ArgName, string a_Default)
    {
      a_ArgName = a_ArgName.Trim('-');
      return args.FirstOrDefault(arg =>
      {
        return arg.StartsWith($"--{a_ArgName}=")||arg.StartsWith($"-{a_ArgName}=");
       })?.Split('=')[1] ?? a_Default;
    }
    public static string ? GetCmdLine(this string[] args, string a_ArgName)
    {
      a_ArgName = a_ArgName.Trim('-');
      return args.FirstOrDefault(arg =>
      {
        return arg.StartsWith($"--{a_ArgName}=")||arg.StartsWith($"-{a_ArgName}=");
       })?.Split('=')[1] ?? null;
    }
  }
}
