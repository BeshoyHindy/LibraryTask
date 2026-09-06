using Catalog;
using FluentValidation;
using Insights;
using Lending;
using Library.Service;
using Library.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDataSource("librarydb");

builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();
builder.Services.AddGrpcReflection();

// The generator reads this list at compile time, so the assemblies are named by typeof here and
// by ModuleAssemblies.All everywhere else; an architecture test keeps the two in step.
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(CatalogModule), typeof(LendingModule), typeof(InsightsModule)];
    options.PipelineBehaviors = [typeof(ValidationBehaviour<,>)];
});

builder.Services.AddValidatorsFromAssemblies(ModuleAssemblies.All);
builder.AddModules(ModuleAssemblies.All);

var app = builder.Build();

app.MapModules(ModuleAssemblies.All);
app.MapGrpcHealthChecksService();
app.MapHealthChecks("/health");
app.MapGrpcReflectionService();

app.Run();
