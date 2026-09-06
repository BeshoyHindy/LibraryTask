using Npgsql;
using Xunit;

namespace Library.TestSupport;

/// <summary>A database of its own for one test class, cloned from the seeded template.</summary>
public sealed class TestDatabase : IAsyncLifetime
{
    private readonly string name = $"library_{Guid.NewGuid():N}";

    private string administrationConnectionString = string.Empty;

    public string ConnectionString { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        administrationConnectionString = await TemplateDatabase.AdministrationConnectionStringAsync();

        NpgsqlConnection.ClearAllPools();
        await TemplateDatabase.ExecuteAsync(
            administrationConnectionString,
            $"""CREATE DATABASE "{name}" TEMPLATE "{TemplateDatabase.Name}" """);

        ConnectionString = TemplateDatabase.ConnectionStringFor(administrationConnectionString, name);
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await TemplateDatabase.ExecuteAsync(
            administrationConnectionString,
            $"""DROP DATABASE IF EXISTS "{name}" WITH (FORCE)""");
    }
}
