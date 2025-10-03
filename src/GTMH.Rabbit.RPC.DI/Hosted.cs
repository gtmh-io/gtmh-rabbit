using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public static class Hosted
  {
    public static IHostApplicationBuilder AddRPCClientConfig(this IHostApplicationBuilder builder)
    {
      builder.Services.AddOptions<RPCClientConfig>()
        .Bind(builder.Configuration.GetSection(nameof(RPCClientConfig)))
        .ValidateDataAnnotations()
        .ValidateOnStart();
      return builder;
    }
    public static IHostApplicationBuilder AddRPCSharedConfig<T>(this IHostApplicationBuilder builder) where T : class, IRPCTopology
    {
      builder.Services.AddOptions<RPCConfig>()
        .Bind(builder.Configuration.GetSection(nameof(RPCConfig)))
        .ValidateDataAnnotations()
        .ValidateOnStart();
      builder.Services.AddSingleton<IRPCFactory, RPCFactory>();
      builder.Services.AddSingleton<IRPCTopology, T>();
      return builder;
    }
    public static IHostApplicationBuilder AddRPCSharedConfig(this IHostApplicationBuilder builder)
    {
      builder.Services.AddOptions<RPCConfig>()
        .Bind(builder.Configuration.GetSection(nameof(RPCConfig)))
        .ValidateDataAnnotations()
        .ValidateOnStart();
      builder.Services.AddSingleton<IRPCFactory, RPCFactory>();
      builder.Services.AddSingleton<IRPCTopology, BasicTopology>();
      return builder;
    }
  }
}
