using Grpc.Core;

using GrpcCommon;

using GTMH.GRPC.Discovery;

namespace GrpcWorkerService;

public class ServerImpl : HelloWorld.HelloWorldBase
{
  private readonly ILogger<ServerImpl> Log;

  public ServerImpl(ILogger<ServerImpl> logger)
  {
    Log = logger;
  }

  public override Task<HelloReply> Introducing(HelloRequest request, ServerCallContext context)
  {
    Log.LogInformation($"Received request from {request.Name}");

    return Task.FromResult(new HelloReply
    {
      Message = $"Hello {request.Name} from gRPC Worker Service!"
    });
  }
}
