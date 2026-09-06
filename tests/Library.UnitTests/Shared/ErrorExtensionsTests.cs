using Google.Rpc;
using Grpc.Core;
using Library.Shared;
using Xunit;

namespace Library.UnitTests.Shared;

public sealed class ErrorExtensionsTests
{
    [Fact]
    public void ToRpcException_NotFound_IsNotFoundWithTheMessage()
    {
        var exception = Error.NotFound("Book 99 was not found.").ToRpcException();

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Equal("Book 99 was not found.", exception.Status.Detail);
    }

    [Fact]
    public void ToRpcException_Conflict_IsFailedPrecondition()
    {
        var exception = Error.Conflict("Book 3 is already on loan.").ToRpcException();

        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
        Assert.Equal("Book 3 is already on loan.", exception.Status.Detail);
    }

    [Fact]
    public void ToRpcException_Validation_IsInvalidArgument()
    {
        var error = Error.Validation([new FieldError("limit", "must be between 1 and 100.")]);

        var exception = error.ToRpcException();

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public void ToRpcException_Validation_CarriesABadRequestDetailPerField()
    {
        var error = Error.Validation(
        [
            new FieldError("limit", "must be between 1 and 100."),
            new FieldError("from", "must not be after to."),
        ]);

        var exception = error.ToRpcException();

        var violations = exception.GetRpcStatus()?.GetDetail<BadRequest>()?.FieldViolations;
        Assert.NotNull(violations);
        Assert.Equal(["limit", "from"], violations.Select(violation => violation.Field));
        Assert.Equal(
            ["must be between 1 and 100.", "must not be after to."],
            violations.Select(violation => violation.Description));
    }
}
