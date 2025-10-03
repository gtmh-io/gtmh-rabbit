using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public interface IRPCClientConfig
  {
    int CallTimeout { get; }
    int ConnectTimeout { get; }
  }
  public class RPCClientConfig : IRPCClientConfig
  {
    public int CallTimeout { get; set; } = 10000;
    public int ConnectTimeout { get; set; } = 10000;
    public static Dictionary<string, string> GetCommandLineMappings()
    {
      var rval = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      rval.Add( "-rpc.timeout", $"{nameof(RPCClientConfig)}:{nameof(RPCClientConfig.CallTimeout)}");
      rval.Add( "--rpc.timeout", $"{nameof(RPCClientConfig)}:{nameof(RPCClientConfig.CallTimeout)}");
      rval.Add( "-rpc.connect_timeout", $"{nameof(RPCClientConfig)}:{nameof(RPCClientConfig.ConnectTimeout)}");
      rval.Add( "--rpc.connect_timeout", $"{nameof(RPCClientConfig)}:{nameof(RPCClientConfig.ConnectTimeout)}");
      return rval;
    }

    public static Dictionary<string, string> WithCommandLineMappings(Dictionary<string, string> a_Other)
    {
      var rval = GetCommandLineMappings();
      foreach(var kvp in a_Other) rval.Add(kvp.Key, kvp.Value);
      return rval;
    }
  }
}
