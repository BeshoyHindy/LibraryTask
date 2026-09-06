using System.Net.Http.Json;
using Library.TestSupport;
using Xunit;

namespace Library.SystemTests;

/// <summary>The HTTP host over the gRPC host, both in process on a database cloned from the seeded template.</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly TestDatabase database = new();

    private ServiceHostFactory? service;
    private ApiHostFactory? api;
    private HttpClient? client;

    public async ValueTask InitializeAsync()
    {
        await database.InitializeAsync();

        service = new ServiceHostFactory(database.ConnectionString);
        api = new ApiHostFactory(service);
        client = api.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        client?.Dispose();

        // The API host holds handlers of the service host's test server, so it goes first.
        api?.Dispose();
        service?.Dispose();

        await database.DisposeAsync();
    }

    public Task<HttpResponseMessage> GetAsync(string route) =>
        Client.GetAsync(new Uri(route, UriKind.Relative), TestContext.Current.CancellationToken);

    public Task<HttpResponseMessage> PostAsJsonAsync<T>(string route, T body) =>
        Client.PostAsJsonAsync(new Uri(route, UriKind.Relative), body, TestContext.Current.CancellationToken);

    private HttpClient Client => client ?? throw new InvalidOperationException("The API host has not been started.");
}
