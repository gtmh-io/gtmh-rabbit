using GTMH.DI.Security;
using GTMH.Security;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GTMH.DI;

public static class Hosted
{
  public static IHostApplicationBuilder AddGTMHCipherEncryption(this IHostApplicationBuilder builder)
  {
    builder.Services.AddSingleton<IDecryptor, GTMH.DI.Security.CipherEncryption>();
    builder.Services.AddOptions<CipherConfig>()
      .Bind(builder.Configuration.GetSection(nameof(CipherConfig)))
      .ValidateDataAnnotations()
      .ValidateOnStart();
    return builder;
  }

  public static IHostApplicationBuilder AddGTMHConfig(this IHostApplicationBuilder builder)
  {
    return builder.AddGTMHConfig(null, null);
  }

  public static IHostApplicationBuilder AddGTMHConfig(this IHostApplicationBuilder builder, string[] args, Dictionary<string, string> a_MappingsA, Dictionary<string,string> a_MappingsB, params Dictionary<string,string> [] a_VA)
  {
    var mappings = new Dictionary<string, string>();
    foreach (var kvp in a_MappingsA) mappings.Add(kvp.Key, kvp.Value);
    foreach (var kvp in a_MappingsB) mappings.Add(kvp.Key, kvp.Value);
    foreach(var mapping in a_VA)
    {
      foreach (var kvp in mapping) mappings.Add(kvp.Key, kvp.Value);
    }
    return builder.AddGTMHConfig(args, mappings);
  }

  public static IHostApplicationBuilder AddGTMHConfig(this IHostApplicationBuilder builder, string [] ? args, Dictionary<string, string> ? a_CmdLineMappings = null)
  {
    builder.Configuration
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
      .AddEnvironmentVariables()
      .AddCommandLine(args??Array.Empty<string>(), a_CmdLineMappings);
      return builder;
  }
  public static IHostApplicationBuilder SetPlaformService(this IHostApplicationBuilder builder, string ? a_ServiceName=null)
  {
#if WINDOWS
    if ( a_ServiceName == null ) throw new ArgumentNullException("Expect a service name");
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = a_ServiceName;
    });
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
  builder.Services.AddSystemd();
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
