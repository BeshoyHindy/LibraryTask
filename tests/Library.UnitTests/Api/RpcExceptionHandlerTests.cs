using Grpc.Core;
using Library.Api;
using Xunit;

namespace Library.UnitTests.Api;

public sealed class RpcExceptionHandlerTests
{
    [Theory]
    [InlineData(StatusCode.NotFound, 404)]
    [InlineData(StatusCode.FailedPrecondition, 409)]
    [InlineData(StatusCode.InvalidArgument, 400)]
    [InlineData(StatusCode.Unavailable, 503)]
    public void ToHttpStatus_AStatusTheServiceReturns_MapsToItsHttpStatus(StatusCode status, int expected)
    {
        var httpStatus = RpcExceptionHandler.ToHttpStatus(status);

        Assert.Equal(expected, httpStatus);
    }

    [Theory]
    [InlineData(StatusCode.Internal)]
    [InlineData(StatusCode.Unknown)]
    [InlineData(StatusCode.Unimplemented)]
    public void ToHttpStatus_AnythingElse_IsABadGateway(StatusCode status)
    {
        var httpStatus = RpcExceptionHandler.ToHttpStatus(status);

        Assert.Equal(502, httpStatus);
    }
}
