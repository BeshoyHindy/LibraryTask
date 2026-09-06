using Catalog.Data;
using Library.Migrations;
using Library.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Library.IntegrationTests.Catalog;

public sealed class CatalogSeedTests(TestDatabase database) : IClassFixture<TestDatabase>, IDisposable
{
    private readonly CatalogDbContext dbContext = CatalogDbContextFactory.Create(database.ConnectionString);

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task ApplyAsync_LeavesTwelveBooks()
    {
        var books = await dbContext.Books.CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(12, books);
    }

    [Fact]
    public async Task ApplyAsync_OnAnAlreadySeededCatalog_InsertsNothing()
    {
        await CatalogSeed.ApplyAsync(dbContext, TestContext.Current.CancellationToken);

        Assert.Equal(12, await dbContext.Books.CountAsync(TestContext.Current.CancellationToken));
    }
}
