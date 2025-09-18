using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit
{
  public static class Hosted
  {
    public static IHostApplicationBuilder AddRabbitConfig(this IHostApplicationBuilder builder)
    {
      builder.Services.AddOptions<RabbitConfig>()
        .Bind(builder.Configuration.GetSection(nameof(RabbitConfig)))
        .ValidateDataAnnotations()
        .ValidateOnStart();
      builder.Services.AddSingleton<IRabbitFactory, RabbitFactory>();
      return builder;
    }
    public static IHostApplicationBuilder AddMQSource<M, F, T>(this IHostApplicationBuilder builder)
      where F : class, IMessageStreamSourceFactory<M>
      where T : class, IMQTopology<M>
    {
      builder.Services.AddSingleton<IMQTopology<M>, T>();
      builder.Services.AddSingleton<IMessageStreamSourceFactory<M>, F>();
      return builder;
    }
    public static IHostApplicationBuilder AddMQSink<M, F, T>(this IHostApplicationBuilder builder)
      where F : class, IMessageStreamSinkFactory<M>
      where T : class, IMQTopology<M>
    {
      builder.Services.AddSingleton<IMQTopology<M>, T>();
      builder.Services.AddSingleton<IMessageStreamSinkFactory<M>, F>();
      return builder;
    }
  }
}
