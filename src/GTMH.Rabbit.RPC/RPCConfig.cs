using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  public class RPCConfig
  {
    public const int DefaultServerQueueTTL = 60000;
    public const ushort DefaultServerMaxConcurrency = ushort.MaxValue;
    [Required]
    public required RabbitConfig Transport { get; set; }
    public int ServerQueueTTL { get; set; } = DefaultServerQueueTTL;
    public ushort ServerMaxConcurrency { get; set; } = DefaultServerMaxConcurrency;
  }
}
