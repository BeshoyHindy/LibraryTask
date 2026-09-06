using Catalog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Library.Migrations;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public static CatalogDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>();
        CatalogDbContext.Configure(options, connectionString);

        return new CatalogDbContext(options.Options);
    }

    public CatalogDbContext CreateDbContext(string[] args) => Create(DesignTimeConnection.String);
}
