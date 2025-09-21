using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.Impl
{
  public class HostOnlyFactory(string a_HostName) : IRabbitFactory
  {
    public string HostIdentity { get; } = a_HostName;
    public IConnectionFactory Create()=>new ConnectionFactory { HostName = HostIdentity };
  }
}
