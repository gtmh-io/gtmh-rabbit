using Grpc.Core;

using GrpcCommon;

namespace GrpcWorkerService;

public class ServerImpl : HelloWorld.HelloWorldBase
{
    private readonly ILogger<ServerImpl> _logger;

    public ServerImpl(ILogger<ServerImpl> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> Introducing(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Received request from {request.Name}");
        
        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name} from gRPC Worker Service!"
        });
    }
}
