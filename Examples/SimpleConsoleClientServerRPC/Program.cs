using GTMH.IDL;
using GTMH.Rabbit;
using GTMH.Rabbit.RPC;

using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

// the interface we're exposing
[RPCInterface]
public interface IHelloWorld
{
  [RPCMethod]
  ValueTask<string> IntroducingAsync(string a_Identity);
}

// implementation of the server
public class ServerImpl(string a_Identity) : IHelloWorld
{
  public readonly string Identity = a_Identity;
  public ValueTask<string> IntroducingAsync(string a_Identity) => ValueTask.FromResult($"Hello {a_Identity}, my name is {Identity}");
}

public static class Program
{
  // will run as either client or server
  public static async Task Main(string [] args)
  {
    var identity = args.FirstOrDefault(arg => arg.StartsWith("--identity="))?.Split('=')[1] ?? "roger";
    var rabbit = args.FirstOrDefault(arg => arg.StartsWith("--rabbit="))?.Split('=')[1] ?? "localhost";
    var runas = (args.FirstOrDefault(arg => arg.StartsWith("--runas="))?.Split('=')[1] ?? "client").ToLower();

    Console.WriteLine($"I am {identity} connecting to Rabbit@{rabbit}");
    if(runas == "client")
    {
      await RunClient(identity, new RPCFactory(new HostOnlyFactory(rabbit)));
    }
    else
    {
      await RunServer(identity, new RPCFactory(new HostOnlyFactory(rabbit)));
    }
  }

  private static async ValueTask RunServer(string identity, IRPCFactory rabbit)
  {
    Console.WriteLine("Running as server, hit enter to quit...");
    var serverImpl = new ServerImpl(identity);
    var serverHost = new HelloWorldDispatch(rabbit, new BasicTopology(), new NullLogger(), serverImpl);
    var server = await serverHost.Publish();
    await using(server)
    {
      Console.ReadLine();
    }
  }

  private static async ValueTask RunClient(string identity, IRPCFactory rabbit)
  {
    Console.WriteLine("Running as client...");
    var client = new HelloWorldClient(rabbit);
    await client.Connect();
    var result = await client.IntroducingAsync(identity);
    Console.WriteLine($"RPC pleasantries... {result}");
  }
}

// Because we're not using dependency injection we need this
public class HostOnlyFactory(string a_HostName) : IRabbitFactory
{
  public string HostIdentity { get; } = a_HostName;
  public IConnectionFactory Create()=>new ConnectionFactory { HostName = HostIdentity };
}

// Because we're not using dependency injection we need this
public class NullLogger : ILogger
{
  public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
  public bool IsEnabled(LogLevel logLevel) => false;
  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
