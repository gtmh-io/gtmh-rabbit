using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GTMH.Rabbit
{
  public class RabbitConfig
  {
    [Required]
    public required string Host { get; set; }
    public string? Password { get; set; }
    public string? User { get; set; }
    public string Identity => User==null?Host:$"{User}@{Host}";
    public static Dictionary<string, string> GetCommandLineMappings()
    {
      var rval = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      rval.Add( "-rabbit.host", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.Host)}");
      rval.Add( "--rabbit.host", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.Host)}");
      rval.Add( "-rabbit.user", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.User)}");
      rval.Add( "--rabbit.user", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.User)}");
      rval.Add( "-rabbit.password", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.Password)}");
      rval.Add( "--rabbit.password", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.Password)}");
      return rval;
    }
  }
}
