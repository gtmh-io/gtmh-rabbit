using Grpc.Core;

using GrpcCommon;

using GTMH.GRPC.Discovery;

namespace GrpcWorkerService;

public class ServerImpl : HelloWorld.HelloWorldBase
{
  private readonly ILogger<ServerImpl> _logger;
  private readonly IDiscoveryService<HelloWorld.HelloWorldClient> Discovery;

  public ServerImpl(ILogger<ServerImpl> logger, IDiscoveryService<HelloWorld.HelloWorldClient> a_Discovery)
  {
    _logger = logger;
    this.Discovery=a_Discovery;
  }

  public Task<IAsyncDisposable> Publish()=>this.Discovery.Publish();

  public override Task<HelloReply> Introducing(HelloRequest request, ServerCallContext context)
  {
    _logger.LogInformation($"Received request from {request.Name}");

    return Task.FromResult(new HelloReply
    {
      Message = $"Hello {request.Name} from gRPC Worker Service!"
    });
  }
}
