using Google.Rpc;
using Grpc.Core;
using Xunit;

namespace Library.FunctionalTests;

internal static class AsyncUnaryCallExtensions
{
    public static async Task<RpcException> FailureAsync<TResponse>(this AsyncUnaryCall<TResponse> call) =>
        await Assert.ThrowsAsync<RpcException>(() => call.ResponseAsync);

    public static IReadOnlyList<string> ViolatedFields(this RpcException exception)
    {
        var violations = exception.GetRpcStatus()?.GetDetail<BadRequest>()?.FieldViolations;
        Assert.NotNull(violations);

        return [.. violations.Select(violation => violation.Field)];
    }
}
