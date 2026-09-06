using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;

namespace Library.Shared;

public static class ErrorExtensions
{
    public static RpcException ToRpcException(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error.Kind switch
        {
            ErrorKind.NotFound => new RpcException(new Grpc.Core.Status(StatusCode.NotFound, error.Message)),
            ErrorKind.Conflict => new RpcException(new Grpc.Core.Status(StatusCode.FailedPrecondition, error.Message)),
            ErrorKind.Validation => InvalidArgument(error),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error.Kind, "Unknown error kind."),
        };
    }

    private static RpcException InvalidArgument(Error error)
    {
        var badRequest = new BadRequest();
        foreach (var field in error.Fields)
        {
            badRequest.FieldViolations.Add(new BadRequest.Types.FieldViolation
            {
                Field = field.Field,
                Description = field.Message,
            });
        }

        var status = new Google.Rpc.Status
        {
            Code = (int)Code.InvalidArgument,
            Message = error.Message,
            Details = { Any.Pack(badRequest) },
        };

        return status.ToRpcException();
    }
}
