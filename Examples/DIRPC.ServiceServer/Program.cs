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

var environmentName = GTMH.DI.Hosted.GetEnvironmenName(args);

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

  builder.SetPlaformService("DROPS.TestWorker");

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
