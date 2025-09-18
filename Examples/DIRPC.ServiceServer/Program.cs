using DIRPC.ServiceServer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if WINDOWS
using Microsoft.Extensions.Hosting.WindowsServices;
#elif LINUX
using Microsoft.Extensions.Hosting.Systemd;
#endif


using Serilog;

using GTMH.DI;
using GTMH.Rabbit.RPC;
using GTMH.Security;

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

// let's have logging in case of error
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
  Log.Information($"Starting server PId={System.Diagnostics.Process.GetCurrentProcess().Id} PWD={System.IO.Directory.GetCurrentDirectory()}");

  // do not pass args to application builder
  var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { ContentRootPath = AppContext.BaseDirectory, EnvironmentName = environmentName } );
  builder.Services.AddHostedService<Worker>();

#if WINDOWS
  builder.Services.AddWindowsService(options =>
  {
      options.ServiceName = "DROPS.TestWorker"; // TODO this is dodgy AF
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


  // GTMH sepcific config
  // have appsettings stuff read
  builder.AddGTMHConfig(args, RPCClientConfig.GetCommandLineMappings());
  // no decryption
  builder.Services.AddSingleton<IDecryptor, PlainText>();
  // shared config
  builder.AddRPCSharedConfig();

  // configure serilog
  builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

  var host = builder.Build();
  host.Run();
  return 0;
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
  if ( System.Diagnostics.Debugger.IsAttached && !args.Contains("--nobreak") ) System.Diagnostics.Debugger.Break();
  return 1;
}
finally
{
  Log.Information("Application shutting down");
  Log.CloseAndFlush();
}
