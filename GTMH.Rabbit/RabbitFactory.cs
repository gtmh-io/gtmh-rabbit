using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit
{
  public interface IRabbitFactory
  {
    string HostIdentity { get; }
    IConnectionFactory Create();
  }
  public class RabbitFactory : IRabbitFactory
  {
    public RabbitConfig Config { get; }
    public IDecryptor Decryptor { get; }

    public string HostIdentity => Config.Host; // TODO - add virtualhost information

    public RabbitFactory(IOptions<RabbitConfig> a_Config, IDecryptor a_Decryptor)
    {
      this.Config = a_Config.Value;
      this.Decryptor= a_Decryptor;
    }
    public RabbitFactory(RabbitConfig a_Config, IDecryptor a_Decryptor)
    {
      this.Config = a_Config;
      this.Decryptor = a_Decryptor;
    }

    public IConnectionFactory Create()
    {
      return new RabbitMQ.Client.ConnectionFactory { HostName = Config.Host };
    }
  }
}
