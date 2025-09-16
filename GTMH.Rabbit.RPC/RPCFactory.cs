using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public interface IRPCFactory
  {
    IRabbitFactory Transport { get; }
    int ServerQueueTTL { get; }
    ushort ServerMaxConcurrency { get; }
  }
  public class RPCFactory : IRPCFactory
  {
    public IRabbitFactory Transport { get; }
    public int ServerQueueTTL { get; }
    public ushort ServerMaxConcurrency { get; }

    public RPCFactory(IOptions<RPCConfig> a_Config, IDecryptor a_Decryptor)
    {
      Transport = new RabbitFactory(a_Config.Value.Transport, a_Decryptor);
      ServerQueueTTL = a_Config.Value.ServerQueueTTL;
      ServerMaxConcurrency = a_Config.Value.ServerMaxConcurrency;
    }
  }
}
