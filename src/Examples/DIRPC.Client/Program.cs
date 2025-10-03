using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;


using DIRPC.Client;

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
    Log.Information("Starting client...");
  // do not pass args to application builder
  var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { ContentRootPath = AppContext.BaseDirectory, EnvironmentName = environmentName } );

  // GTMH sepcific config
  // have appsettings stuff read
  builder.AddStdConfig(args, RPCClientConfig.GetCommandLineMappings());
  // no decryption
  builder.Services.AddSingleton<IDecryptor, PlainText>();
  // setup client config
  builder.AddRPCClientConfig();
  // shared config
  builder.AddRPCSharedConfig();

  // configure serilog
  builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

  // standard DI config
  builder.Services.AddSingleton<Main>();
  using var app = builder.Build();
  var main = app.Services.GetRequiredService<Main>();
  await using(main)
  {
    return await main.RunAsync();
  }
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

