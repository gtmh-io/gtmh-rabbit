using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GTMH.DI;

public static class Hosted
{
  public static IHostApplicationBuilder AddGTMHConfig(this IHostApplicationBuilder builder, string [] ? args = null, Dictionary<string, string> ? a_CmdLineMappings = null)
  {
    builder.Configuration
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
      .AddEnvironmentVariables()
      .AddCommandLine(args??Array.Empty<string>(), a_CmdLineMappings);
      return builder;
  }
}
