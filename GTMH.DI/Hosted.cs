using GTMH.Security;
using GTMH.Util;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GTMH.DI;

public static class Hosted
{
  public static IHostApplicationBuilder AddStdConfig(this IHostApplicationBuilder builder)=> builder.AddStdConfig(null);
  public static IHostApplicationBuilder AddStdConfig(this IHostApplicationBuilder builder, string [] ? args) => AddStdConfig(builder, args, Array.Empty<Dictionary<string, string>>());
  public static IHostApplicationBuilder AddStdConfig(this IHostApplicationBuilder builder, string[]? args, Dictionary<string, string> a_CmdLineMappings)=>AddStdConfig(builder, args, new Dictionary<string, string>[] { a_CmdLineMappings });
  public static IHostApplicationBuilder AddStdConfig(this IHostApplicationBuilder builder, string[]? args, IEnumerable<Dictionary<string, string>> a_CmdLineMappings)
  {
    // hoping for consistency
    var mappings = new Dictionary<string, string>();
    foreach(var mapping_set in a_CmdLineMappings)
    {
      foreach(var kvp in mapping_set)
      {
        if(mappings.TryGetValue(kvp.Key, out var currValue))
        {
          if(currValue != kvp.Value) throw new ArgumentException($"'{kvp.Key}' has multiple conflicting definitions");
        }
        else
        {
          mappings.Add(kvp.Key, kvp.Value);
        }
      }
    }
    builder.Configuration
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
      .AddEnvironmentVariables()
      .AddCommandLine(args??Array.Empty<string>(), mappings);
    return builder;

  }

  public static IHostApplicationBuilder SetPlaformService(this IHostApplicationBuilder builder, string [] a_Args)
  {
#if WINDOWS
    if(!a_Args.HasCmdLineFlag("--console"))
    {
      var a_ServiceName = a_Args.GetCmdLine("WindowsServiceName");
      if(a_ServiceName == null)
        throw new ArgumentNullException("Expect either --console or a service name specified via --WindowsServiceName=<the_service_name>");
      builder.Services.AddWindowsService(options =>
      {
        options.ServiceName = a_ServiceName;
      });
    }
    if (System.IO.Directory.GetCurrentDirectory().ToLower().StartsWith(@"c:\windows"))
    {
      // deal with windows starting services in dumb places
      var loc = System.Reflection.Assembly.GetEntryAssembly()?.Location;
      if(loc != null)
      {
        loc = System.IO.Path.GetDirectoryName(loc);
        if(loc != null)
        {
          System.IO.Directory.SetCurrentDirectory(loc);
        }
      }
    }
#elif LINUX
  if(!a_Args.HasCmdLineFlag("--console"))
  {
    builder.Services.AddSystemd();
  }
#endif
    return builder;
  }

  public static string GetEnvironmenName(string [] args)
  {
    var environmentName = args.FirstOrDefault(arg => arg.StartsWith("--environment="))?.Split('=')[1] ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    #if DEBUG
    // hosting environment is set via the environment variable DOTNET_ENVIRONMENT and defaults to Production
    if(environmentName == null) // can force
    {
      environmentName="Development";
    }
    #else
    if(environmentName == null) // can force
    {
      environmentName = "Production";
    }
    #endif
    return environmentName;
  }
}
