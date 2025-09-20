using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public static class Hosted
  {
    public static IHostApplicationBuilder AddDiscoveryConfig(this IHostApplicationBuilder builder)
    {
      builder.Services.AddOptions<DiscoveryConfig>()
        .Bind(builder.Configuration.GetSection(nameof(DiscoveryConfig)))
        .ValidateDataAnnotations()
        .ValidateOnStart();

      return builder;
    }
  }
}
