using GTMH.Rabbit;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GTMH.GRPC.Discovery
{
  public class DiscoveryConfig
  {
    [Required]
    public required RabbitConfig Transport { get; set; }
  }
}
