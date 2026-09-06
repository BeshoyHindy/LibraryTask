using Library.TestSupport;
using Xunit;

namespace Library.FunctionalTests;

/// <summary>The gRPC host in process over a database cloned from the seeded template, one per test class.</summary>
public sealed class ServiceFixture : IAsyncLifetime
{
    private readonly TestDatabase database = new();

    private ServiceHostFactory? host;

    public ServiceHostFactory Host => host ?? throw new InvalidOperationException("The service host has not been started.");

    public async ValueTask InitializeAsync()
    {
        await database.InitializeAsync();

        host = new ServiceHostFactory(database.ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        host?.Dispose();
        await database.DisposeAsync();
    }
}
