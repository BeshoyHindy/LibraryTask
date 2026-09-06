using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Catalog.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";

    public DbSet<Book> Books => Set<Book>();

    public static void Configure(DbContextOptionsBuilder options, NpgsqlDataSource dataSource) =>
        options.UseNpgsql(dataSource, Npgsql);

    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, Npgsql);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new BookConfiguration());
    }

    // The history table does not follow HasDefaultSchema, so every context that migrates has to
    // name its own, or the two modules share one table in public.
    private static void Npgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql
            .MigrationsAssembly("Library.Migrations")
            .MigrationsHistoryTable("__EFMigrationsHistory", Schema);
}
