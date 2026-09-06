using Lending.Contracts;
using Lending.Data;
using Lending.Grpc;
using Library.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

[assembly: LibraryModule(typeof(Lending.LendingModule))]

namespace Lending;

public sealed class LendingModule : IModule
{
    public void AddServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddDbContext<LendingDbContext>((serviceProvider, options) =>
            LendingDbContext.Configure(options, serviceProvider.GetRequiredService<NpgsqlDataSource>()));

        builder.Services.AddScoped<IBorrowerReader, BorrowerReader>();
        builder.Services.AddScoped<ILoanReader, LoanReader>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGrpcService<LendingGrpcService>();
    }
}
