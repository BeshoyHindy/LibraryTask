using Catalog.Data;
using Lending.Data;
using Library.Migrations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Library.TestSupport;

// Migrating and seeding costs seconds; cloning the result costs milliseconds, so the assembly pays
// for it once and every test class starts from a copy.
public static class TemplateDatabase
{
    public const string Name = "library_template";

    private static readonly Lazy<Task<string>> Prepared = new(PrepareAsync);

    /// <summary>Ensures the template exists and returns a connection string for the server's own database.</summary>
    public static Task<string> AdministrationConnectionStringAsync() => Prepared.Value;

    public static string ConnectionStringFor(string administrationConnectionString, string database) =>
        new NpgsqlConnectionStringBuilder(administrationConnectionString) { Database = database }.ConnectionString;

    public static async Task ExecuteAsync(string connectionString, string sql)
    {
        // Pooling would keep a session on the database this statement is about to clone or drop.
        var unpooled = new NpgsqlConnectionStringBuilder(connectionString) { Pooling = false }.ConnectionString;

        await using var connection = new NpgsqlConnection(unpooled);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<string> PrepareAsync()
    {
        var container = await PostgresContainer.GetAsync();
        var administration = container.GetConnectionString();

        await ExecuteAsync(administration, $"""CREATE DATABASE "{Name}" """);

        var template = ConnectionStringFor(administration, Name);

        await using (var catalog = CatalogDbContextFactory.Create(template))
        {
            await catalog.Database.MigrateAsync();
            await CatalogSeed.ApplyAsync(catalog, CancellationToken.None);
        }

        await using (var lending = LendingDbContextFactory.Create(template))
        {
            await lending.Database.MigrateAsync();
            await LendingSeed.ApplyAsync(lending, CancellationToken.None);
        }

        // CREATE DATABASE ... TEMPLATE fails while any session is connected to the template, and
        // EF leaves its connections in the pool.
        NpgsqlConnection.ClearAllPools();

        return administration;
    }
}
