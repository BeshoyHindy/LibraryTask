using Lending.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Library.Migrations;

public sealed class LendingDbContextFactory : IDesignTimeDbContextFactory<LendingDbContext>
{
    public static LendingDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LendingDbContext>();
        LendingDbContext.Configure(options, connectionString);

        return new LendingDbContext(options.Options);
    }

    public LendingDbContext CreateDbContext(string[] args) => Create(DesignTimeConnection.String);
}
