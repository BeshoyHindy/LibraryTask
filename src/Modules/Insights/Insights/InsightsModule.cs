using Insights.Grpc;
using Library.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

[assembly: LibraryModule(typeof(Insights.InsightsModule))]

namespace Insights;

public sealed class InsightsModule : IModule
{
    // Insights owns no tables and no services of its own; its handlers compose the readers the
    // other two modules register.
    public void AddServices(IHostApplicationBuilder builder)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGrpcService<InsightsGrpcService>();
    }
}
