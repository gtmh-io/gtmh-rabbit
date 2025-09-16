using GTMH.IDL;
using GTMH.Rabbit;
using GTMH.Rabbit.RPC;

using RabbitMQ.Client;
using System.Threading.Tasks;

// the interface we're exposing
[RPCInterface]
interface IHelloWorld
{
  [RPCMethod]
  ValueTask<string> IntroducingAsync(string a_Identity);
}

// implementation of the server
public class ServerImpl : IHelloWorld
{
  public ValueTask<string> IntroducingAsync(string a_Identity) => ValueTask.FromResult($"Hello {a_Identity}, I am a Server");
}

public static class Program
{
  // will run as either client or server depending on value of --runas=type
  public static async Task Main(string [] args)
  {
    var rabbit = args.FirstOrDefault(arg => arg.StartsWith("--rabbit="))?.Split('=')[1] ?? "localhost";
    var runas = (args.FirstOrDefault(arg => arg.StartsWith("--runas="))?.Split('=')[1] ?? "client").ToLower();

    if(runas == "client") await RunClient(new RPCFactory(new HostOnlyFactory(rabbit)));
    else await RunServer(new RPCFactory(new HostOnlyFactory(rabbit)));
  }

  private static async ValueTask RunServer(IRPCFactory rabbit)
  {
    Console.WriteLine($"Running as server@{rabbit.Transport.HostIdentity}, hit enter to quit...");
    var serverHost = new HelloWorldServiceHost(rabbit, new ServerImpl());
    var server = await serverHost.Publish();
    await using(server)
    {
      Console.ReadLine();
    }
  }

  private static async ValueTask RunClient(IRPCFactory rabbit)
  {
    Console.WriteLine($"Running as client@{rabbit.Transport.HostIdentity}...");
    var client = new HelloWorldClient(rabbit);
    await client.Connect();
    await using(client)
    {
      var result = await client.IntroducingAsync("client");
      Console.WriteLine($"RPC pleasantries... {result}");
    }
  }
}

// Because we're not using dependency injection we need this
public class HostOnlyFactory(string a_HostName) : IRabbitFactory
{
  public string HostIdentity { get; } = a_HostName;
  public IConnectionFactory Create()=>new ConnectionFactory { HostName = HostIdentity };
}
