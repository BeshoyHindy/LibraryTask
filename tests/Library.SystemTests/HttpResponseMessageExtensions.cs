using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Library.SystemTests;

internal static class HttpResponseMessageExtensions
{
    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<T>(TestContext.Current.CancellationToken);
        Assert.NotNull(body);

        return body;
    }

    public static Task<ProblemDetails> ProblemAsync(this HttpResponseMessage response, HttpStatusCode expected) =>
        response.ReadProblemAsync<ProblemDetails>(expected);

    public static async Task<IDictionary<string, string[]>> ValidationErrorsAsync(this HttpResponseMessage response)
    {
        var problem = await response.ReadProblemAsync<HttpValidationProblemDetails>(HttpStatusCode.BadRequest);

        return problem.Errors;
    }

    private static async Task<TProblem> ReadProblemAsync<TProblem>(this HttpResponseMessage response, HttpStatusCode expected)
        where TProblem : ProblemDetails
    {
        var problem = await response.ReadAsync<TProblem>(expected);

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal((int)expected, problem.Status);

        return problem;
    }
}
