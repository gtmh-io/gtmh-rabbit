using Grpc.Core;

namespace GrpcWorkerService;

public class ServerImpl : Greeter.GreeterBase
{
    private readonly ILogger<ServerImpl> _logger;

    public ServerImpl(ILogger<ServerImpl> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Received request from {request.Name}");
        
        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name} from gRPC Worker Service!"
        });
    }
}
