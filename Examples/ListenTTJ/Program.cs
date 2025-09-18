using GTMH.Rabbit;
using GTMH.Rabbit.Impl;
using GTMH.Util;

using RabbitMQ.Client;

using Tofye.Racing;

var rabbit_host = args.GetCmdLine("rabbit_host", "localhost");
var rabbit_password = args.GetCmdLine("rabbit_password");
var rabbit_user = args.GetCmdLine("rabbit_user");

var rabbit = new HostFactory(rabbit_host, rabbit_password, rabbit_user);
var with_without=rabbit_password==null?"without":"with";

Console.WriteLine($"Connecting to {rabbit.HostIdentity} {with_without} a password");


using var cts = new CancellationTokenSource();
var sourceFactory = new RabbitStreamSourceFactory<TTJMessage>(rabbit, new LegacyTopology());
var source = await sourceFactory.CreateSource(cts.Token);
await using(source)
{
  await source.AddListenerAsync(null, new Listener());
  Console.WriteLine("Running");
  await Task.Run(()=>Console.ReadLine());
  cts.Cancel();
}


class Listener : IMessageStreamListener<TTJMessage>
{
  public ValueTask OnReceivedAsync(TTJMessage a_Msg)
  {
    Console.WriteLine($"Received {a_Msg}");
    return ValueTask.CompletedTask;
  }
}

class LegacyTopology : TransientQueueTopology_t<TTJMessage>
{
  public override string ExchangeName => "tofye.ttj";

}

class HostFactory(string HostName, string? Secret, string ? User) : IRabbitFactory
{
  public string HostIdentity => User==null?HostName:$"{User}@{HostName}";

  public IConnectionFactory Create()
  {
    var cfDefault = new ConnectionFactory();
    var secret = Secret ?? cfDefault.Password;
    var user = User ?? cfDefault.UserName;
    return new ConnectionFactory { HostName = HostName, Password=secret, UserName=user };
  }
}
