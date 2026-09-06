using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Library.Shared;

public interface IModule
{
    void AddServices(IHostApplicationBuilder builder);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
