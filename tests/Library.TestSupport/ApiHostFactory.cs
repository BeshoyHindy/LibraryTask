using Catalog.Contracts.Grpc;
using Insights.Contracts.Grpc;
using Lending.Contracts.Grpc;
using Library.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Library.TestSupport;

public sealed class ApiHostFactory(ServiceHostFactory serviceFactory) : WebApplicationFactory<ApiMarker>
{
    // A plain host name that service discovery passes through, since the service host answers
    // through its test server's handler whatever the address says.
    private static Uri ServiceAddress { get; } = new("http://library-service");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureTestServices(services =>
        {
            // A second AddGrpcClient for the same client type overrides the host's registration.
            services
                .AddGrpcClient<CatalogService.CatalogServiceClient>(options => options.Address = ServiceAddress)
                .ConfigurePrimaryHttpMessageHandler(serviceFactory.Server.CreateHandler);
            services
                .AddGrpcClient<LendingService.LendingServiceClient>(options => options.Address = ServiceAddress)
                .ConfigurePrimaryHttpMessageHandler(serviceFactory.Server.CreateHandler);
            services
                .AddGrpcClient<InsightsService.InsightsServiceClient>(options => options.Address = ServiceAddress)
                .ConfigurePrimaryHttpMessageHandler(serviceFactory.Server.CreateHandler);
        });
    }
}
