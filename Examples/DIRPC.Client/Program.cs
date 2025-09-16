using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;


using DIRPC.Client;

using GTMH.DI;
using GTMH.Rabbit.RPC;

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
  var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
  {
    ContentRootPath = AppContext.BaseDirectory,
    EnvironmentName = environmentName
  } );
  builder.AddGTMHConfig(args, RPCClientConfig.GetCommandLineMappings());

  builder.Services.AddSingleton<Main>();

  using var app = builder.Build();
  var main = app.Services.GetRequiredService<Main>();
  await using(main)
  {
    return await main.RunAsync(args);
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

