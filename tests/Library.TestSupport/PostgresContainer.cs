using Testcontainers.PostgreSql;

namespace Library.TestSupport;

// One server per test assembly. Nothing stops it: Ryuk removes the container, and every database
// in it, when the test run ends.
public static class PostgresContainer
{
    private const string Image = "postgres:18.6-alpine";

    private static readonly Lazy<Task<PostgreSqlContainer>> Running = new(StartAsync);

    public static Task<PostgreSqlContainer> GetAsync() => Running.Value;

    private static async Task<PostgreSqlContainer> StartAsync()
    {
        var container = new PostgreSqlBuilder(Image).Build();
        await container.StartAsync();

        return container;
    }
}
