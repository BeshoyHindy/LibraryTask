namespace Library.Migrations;

// dotnet ef never connects for the commands this repo runs, so a placeholder is enough unless a
// developer points LIBRARY_CONNECTION_STRING at a real database to apply migrations by hand.
internal static class DesignTimeConnection
{
    public static string String =>
        Environment.GetEnvironmentVariable("LIBRARY_CONNECTION_STRING")
        ?? "Host=localhost;Port=5432;Database=librarydb;Username=postgres;Password=postgres";
}
