using GTMH.Rabbit;
using GTMH.Rabbit.Impl;
using GTMH.Util;

using RabbitMQ.Client;

using Tofye.Infrastructure;
using Tofye.Racing;

var rabbit_host = args.GetCmdLine("rabbit_host", "localhost");
var rabbit_password = args.GetCmdLine("rabbit_password");
var rabbit_user = args.GetCmdLine("rabbit_user");

var rabbit = new HostFactory(rabbit_host, rabbit_password, rabbit_user);
var with_without=rabbit_password==null?"without":"with";

Console.WriteLine($"Connecting to {rabbit.HostIdentity} {with_without} a password");


using var cts = new CancellationTokenSource();
//var sourceFactory = new RabbitStreamSourceFactory<TTJMessage>(rabbit, new LegacyTopology<TTJMessage>("tofye.ttj"));
var sourceFactory = new RabbitStreamSourceFactory<PingMsg>(rabbit, new LegacyTopology<PingMsg>("tofye.ping"));
var source = await sourceFactory.CreateSource(cts.Token);
await using(source)
{
  //await source.AddListenerAsync(null, new Listener<TTJMessage>());
  await source.AddListenerAsync(null, new Listener<PingMsg>());
  Console.WriteLine("Running");
  await Task.Run(()=>Console.ReadLine());
  cts.Cancel();
}


class Listener<M> : IMessageStreamListener<M>
{
  public ValueTask OnReceivedAsync(M a_Msg)
  {
    Console.WriteLine($"Received {a_Msg}");
    return ValueTask.CompletedTask;
  }
}

class LegacyTopology<M>(string a_ExchangeName) : TransientQueueTopology_t<M>
{
  public override string ExchangeName => a_ExchangeName;
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
