using Catalog;
using Catalog.Contracts;
using Catalog.Data;
using Lending;
using Lending.Contracts;
using Lending.Data;
using Library.Migrations;

namespace Library.IntegrationTests.Insights;

/// <summary>The three Contracts readers over one cloned database, as the Insights handlers take them.</summary>
internal sealed class Readers : IDisposable
{
    private readonly LendingDbContext lendingDbContext;
    private readonly CatalogDbContext catalogDbContext;

    public Readers(string connectionString)
    {
        lendingDbContext = LendingDbContextFactory.Create(connectionString);
        catalogDbContext = CatalogDbContextFactory.Create(connectionString);

        Loans = new LoanReader(lendingDbContext);
        Borrowers = new BorrowerReader(lendingDbContext);
        Books = new BookReader(catalogDbContext);
    }

    public ILoanReader Loans { get; }

    public IBorrowerReader Borrowers { get; }

    public IBookReader Books { get; }

    public void Dispose()
    {
        lendingDbContext.Dispose();
        catalogDbContext.Dispose();
    }
}
