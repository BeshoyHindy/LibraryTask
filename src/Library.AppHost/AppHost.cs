var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithImageTag("18.6-alpine");
var librarydb = postgres.AddDatabase("librarydb");

var migrator = builder.AddProject<Projects.Library_DbMigrator>("library-migrator")
    .WithReference(librarydb)
    .WaitFor(librarydb);

var service = builder.AddProject<Projects.Library_Service>("library-service")
    .WithReference(librarydb)
    .WaitForCompletion(migrator)
    .WithHttpHealthCheck("/health", endpointName: "http");

builder.AddProject<Projects.Library_Api>("library-api")
    .WithReference(service)
    .WaitFor(service)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
