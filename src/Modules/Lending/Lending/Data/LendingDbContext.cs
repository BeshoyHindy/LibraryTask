using Lending.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Lending.Data;

public sealed class LendingDbContext(DbContextOptions<LendingDbContext> options) : DbContext(options)
{
    public const string Schema = "lending";

    public DbSet<Borrower> Borrowers => Set<Borrower>();

    public DbSet<Loan> Loans => Set<Loan>();

    public static void Configure(DbContextOptionsBuilder options, NpgsqlDataSource dataSource) =>
        options.UseNpgsql(dataSource, Npgsql);

    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, Npgsql);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new BorrowerConfiguration());
        modelBuilder.ApplyConfiguration(new LoanConfiguration());
    }

    private static void Npgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql
            .MigrationsAssembly("Library.Migrations")
            .MigrationsHistoryTable("__EFMigrationsHistory", Schema);
}
