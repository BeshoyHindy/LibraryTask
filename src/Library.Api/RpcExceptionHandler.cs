using Google.Rpc;
using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api;

public sealed class RpcExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public static int ToHttpStatus(StatusCode status) => status switch
    {
        StatusCode.NotFound => StatusCodes.Status404NotFound,
        StatusCode.FailedPrecondition => StatusCodes.Status409Conflict,
        StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
        StatusCode.Unavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status502BadGateway,
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (exception is not RpcException rpcException)
        {
            return false;
        }

        var statusCode = ToHttpStatus(rpcException.StatusCode);
        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = rpcException,
            ProblemDetails = ProblemDetailsFor(rpcException, statusCode),
        });
    }

    private static ProblemDetails ProblemDetailsFor(RpcException exception, int statusCode)
    {
        var violations = exception.GetRpcStatus()?.GetDetail<BadRequest>()?.FieldViolations;

        if (statusCode != StatusCodes.Status400BadRequest || violations is null or { Count: 0 })
        {
            return new ProblemDetails
            {
                Status = statusCode,
                Detail = exception.Status.Detail,
            };
        }

        var errors = violations
            .GroupBy(violation => violation.Field, StringComparer.Ordinal)
            .ToDictionary(
                field => field.Key,
                field => field.Select(violation => violation.Description).ToArray(),
                StringComparer.Ordinal);

        return new HttpValidationProblemDetails(errors) { Status = statusCode };
    }
}
