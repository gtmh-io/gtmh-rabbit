using Grpc.Core;

using GrpcCommon;

using GTMH.GRPC.Discovery;

namespace GrpcWorkerService;

public class ServerImpl : HelloWorld.HelloWorldBase, IHostedService
{
  private readonly ILogger<ServerImpl> Log;
  private readonly IDiscoveryService<HelloWorld.HelloWorldClient> Discovery;
  private IAsyncDisposable ? m_Publication;

  public ServerImpl(ILogger<ServerImpl> logger, IDiscoveryService<HelloWorld.HelloWorldClient> a_Discovery)
  {
    Log = logger;
    this.Discovery=a_Discovery;
  }

  public override Task<HelloReply> Introducing(HelloRequest request, ServerCallContext context)
  {
    Log.LogInformation($"Received request from {request.Name}");

    return Task.FromResult(new HelloReply
    {
      Message = $"Hello {request.Name} from gRPC Worker Service!"
    });
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    m_Publication = await Discovery.Publish();
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    if(m_Publication != null)
    {
      await m_Publication.DisposeAsync();
    }
  }
}
