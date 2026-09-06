using Catalog.Contracts.Grpc;
using Insights.Contracts.Grpc;
using Lending.Contracts.Grpc;
using Library.Api;
using Library.Api.Catalog;
using Library.Api.Insights;
using Library.Api.Lending;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Service discovery resolves the scheme-less host to Services:library-service:grpc:0, which
// Aspire, docker-compose and appsettings.Development.json each set to their own address.
var serviceAddress = new Uri("http://_grpc.library-service");

builder.Services.AddServiceDiscovery();

builder.Services
    .AddGrpcClient<CatalogService.CatalogServiceClient>(options => options.Address = serviceAddress)
    .AddServiceDiscovery();
builder.Services
    .AddGrpcClient<LendingService.LendingServiceClient>(options => options.Address = serviceAddress)
    .AddServiceDiscovery();
builder.Services
    .AddGrpcClient<InsightsService.InsightsServiceClient>(options => options.Address = serviceAddress)
    .AddServiceDiscovery();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RpcExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

// The API is internal, so the document and its UI are always on.
app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/health");

app.MapBooksEndpoints();
app.MapLoansEndpoints();
app.MapInsightsEndpoints();

app.Run();
