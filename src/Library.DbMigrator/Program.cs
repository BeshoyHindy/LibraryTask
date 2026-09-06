using Catalog.Data;
using Lending.Data;
using Library.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

var connectionString = builder.Configuration.GetConnectionString("librarydb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    logger.LogError("ConnectionStrings:librarydb is not configured.");

    return 1;
}

try
{
    await using (var catalog = CatalogDbContextFactory.Create(connectionString))
    {
        logger.LogInformation("Migrating {Module}.", "Catalog");
        await catalog.Database.MigrateAsync(cancellation.Token);

        logger.LogInformation("Seeding {Module}.", "Catalog");
        await CatalogSeed.ApplyAsync(catalog, cancellation.Token);
    }

    await using (var lending = LendingDbContextFactory.Create(connectionString))
    {
        logger.LogInformation("Migrating {Module}.", "Lending");
        await lending.Database.MigrateAsync(cancellation.Token);

        logger.LogInformation("Seeding {Module}.", "Lending");
        await LendingSeed.ApplyAsync(lending, cancellation.Token);
    }
}
catch (Exception exception)
{
    logger.LogError(exception, "The database is not up to date.");

    return 1;
}

logger.LogInformation("The database is up to date.");

return 0;
