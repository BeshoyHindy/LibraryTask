using Catalog.Contracts;
using Catalog.Data;
using Catalog.Grpc;
using Library.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

[assembly: LibraryModule(typeof(Catalog.CatalogModule))]

namespace Catalog;

public sealed class CatalogModule : IModule
{
    public void AddServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddDbContext<CatalogDbContext>((serviceProvider, options) =>
            CatalogDbContext.Configure(options, serviceProvider.GetRequiredService<NpgsqlDataSource>()));

        builder.Services.AddScoped<IBookReader, BookReader>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGrpcService<CatalogGrpcService>();
    }
}
