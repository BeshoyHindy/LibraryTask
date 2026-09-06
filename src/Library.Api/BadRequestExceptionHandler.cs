using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api;

// A body that cannot be read, or a route value that cannot be parsed, is the caller's mistake and
// answers with the same ProblemDetails shape as every other failure.
public sealed class BadRequestExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (exception is not BadHttpRequestException badRequest)
        {
            return false;
        }

        httpContext.Response.StatusCode = badRequest.StatusCode;

        // The framework's message names the parameter; the JSON exception underneath names the field.
        var detail = badRequest.InnerException is JsonException { Path: { } path }
            ? $"The request body could not be read at {path}."
            : badRequest.Message;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = badRequest,
            ProblemDetails = new ProblemDetails { Status = badRequest.StatusCode, Detail = detail },
        });
    }
}
