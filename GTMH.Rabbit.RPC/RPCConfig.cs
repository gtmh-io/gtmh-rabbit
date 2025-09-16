using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public class RPCConfig
  {
    [Required]
    public required RabbitConfig Transport { get; set; }
    public int ServerQueueTTL { get; set; } = 60000;
    public ushort ServerMaxConcurrency { get; set; } = ushort.MaxValue;
  }
}
