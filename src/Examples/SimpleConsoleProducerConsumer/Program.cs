using GTMH.Rabbit;
using GTMH.Rabbit.Impl;

using ProtoBuf;

using RabbitMQ.Client;

[ProtoContract(SkipConstructor=true)]
public class HelloWorldMsg
{
  [ProtoMember(1)]
  public int Number { get; }
  public HelloWorldMsg(int a_Content) { this.Number=a_Content; }
}

public static class Program
{
  public static async Task Main(string[] args)
  {
    var rabbit = args.FirstOrDefault(arg => arg.StartsWith("--rabbit="))?.Split('=')[1] ?? "localhost";
    var runas = (args.FirstOrDefault(arg => arg.StartsWith("--runas="))?.Split('=')[1] ?? "consumer").ToLower();
    if ( runas =="consumer" ) await RunConsumer(new HostOnlyFactory(rabbit));
    else await RunProducer(new HostOnlyFactory(rabbit));
  }

  private static async ValueTask RunProducer(IRabbitFactory rabbit)
  {
    Console.WriteLine($"Running as producer@{rabbit.HostIdentity} hit enter to quit...");
    using var cts = new CancellationTokenSource();
    var sinkfactory = RabbitStreamSinkFactory.Create(rabbit, TransientQueueTopology.Create<HelloWorldMsg>());
    var sink = await sinkfactory.CreateSink(cts.Token);
    await using(sink)
    {
      var publishTask = Task.Run(async ()=>
      {
        int msgNum = 0;
        while(!cts.Token.IsCancellationRequested)
        {
          await sink.PublishAsync("*", new HelloWorldMsg(++msgNum));
          try { await Task.Delay(1000, cts.Token); } catch(TaskCanceledException) { } }
      });
      await Task.Run(()=>Console.ReadLine());
      cts.Cancel();
      await publishTask;
    }
  }

  class Listener : IMessageStreamListener<HelloWorldMsg>
  {
    public ValueTask OnReceivedAsync(HelloWorldMsg a_Msg)
    {
      Console.WriteLine($"Received HelloWorld={a_Msg.Number}");
      return ValueTask.CompletedTask;
    }
  }

  private static async ValueTask RunConsumer(IRabbitFactory rabbit)
  {
    Console.WriteLine($"Running as consumer@{rabbit.HostIdentity} hit enter to quit...");
    using var cts = new CancellationTokenSource();
    var sourceFactory = new RabbitStreamSourceFactory_t<HelloWorldMsg>(rabbit, TransientQueueTopology.Create<HelloWorldMsg>());
    var source = await sourceFactory.CreateSource(cts.Token);
    await using(source)
    {
      await source.AddListenerAsync(null, new Listener());
      await Task.Run(()=>Console.ReadLine());
      cts.Cancel();
    }
  }
}