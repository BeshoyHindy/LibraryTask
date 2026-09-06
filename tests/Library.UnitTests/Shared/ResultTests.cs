using Library.Shared;
using Xunit;

namespace Library.UnitTests.Shared;

public sealed class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_CarriesTheError()
    {
        var error = Error.NotFound("Book 99 was not found.");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.Same(error, result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromError_IsAFailure()
    {
        Result result = Error.Conflict("Book 3 is already on loan.");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Error?.Kind);
    }

    [Fact]
    public void ImplicitConversion_FromValue_IsASuccessCarryingIt()
    {
        Result<int> result = 41;

        Assert.True(result.IsSuccess);
        Assert.Equal(41, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CarriesNoValue()
    {
        Result<int> result = Error.NotFound("Loan 99 was not found.");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Error?.Kind);
    }

    [Fact]
    public void Value_OnAFailure_Throws()
    {
        var result = Result<int>.Failure(Error.NotFound("Loan 99 was not found."));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Validation_KeepsTheFields()
    {
        var error = Error.Validation([new FieldError("limit", "must be between 1 and 100.")]);

        Assert.Equal(ErrorKind.Validation, error.Kind);
        Assert.Equal("limit", Assert.Single(error.Fields).Field);
    }
}
