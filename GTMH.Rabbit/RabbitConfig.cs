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
    public static Dictionary<string, string> GetCommandLineMappings()
    {
      var rval = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      rval.Add( "-rabbit.host", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.Host)}");
      rval.Add( "--rabbit.host", $"{nameof(RabbitConfig)}:{nameof(RabbitConfig.Host)}");
      return rval;
    }
  }
}
