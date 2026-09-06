using Catalog.Contracts.Grpc;
using Grpc.Net.Client;
using Insights.Contracts.Grpc;
using Lending.Contracts.Grpc;
using Library.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.TestSupport;

public sealed class ServiceHostFactory(string connectionString) : WebApplicationFactory<ServiceMarker>
{
    public CatalogService.CatalogServiceClient CreateCatalogClient() => new(CreateChannel());

    public LendingService.LendingServiceClient CreateLendingClient() => new(CreateChannel());

    public InsightsService.InsightsServiceClient CreateInsightsClient() => new(CreateChannel());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseSetting("ConnectionStrings:librarydb", connectionString);
    }

    // The test server answers in process, so the address is a placeholder and the host's
    // HTTP/2-only Kestrel endpoint never comes into it.
    private GrpcChannel CreateChannel() =>
        GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = Server.CreateHandler() });
}
