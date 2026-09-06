using Insights.Features.GetTopBorrowers;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Insights;

public sealed class GetTopBorrowersQueryHandlerTests : IClassFixture<TestDatabase>, IDisposable
{
    private static readonly DateOnly From = new(2025, 3, 1);
    private static readonly DateOnly To = new(2025, 3, 31);

    private readonly Readers readers;
    private readonly GetTopBorrowersQueryHandler handler;

    public GetTopBorrowersQueryHandlerTests(TestDatabase database)
    {
        readers = new Readers(database.ConnectionString);
        handler = new GetTopBorrowersQueryHandler(readers.Loans, readers.Borrowers);
    }

    public void Dispose() => readers.Dispose();

    // Borrowers 5 and 2 both took three books in March, so "Esther Vance" comes before
    // "Nadia Osei"; 1 and 3 both took two, so "Ada Whitfield" comes before "Bruno Castellanos".
    [Fact]
    public async Task Handle_ARange_RanksBorrowersByTheirLoansInIt()
    {
        var result = await handler.Handle(new GetTopBorrowersQuery(From, To, Limit: null), TestContext.Current.CancellationToken);

        Assert.Equal([5, 2, 1, 3, 4, 6], result.Value.Select(borrower => borrower.BorrowerId));
        Assert.Equal([3, 3, 2, 2, 1, 1], result.Value.Select(borrower => borrower.LoanCount));
    }

    [Fact]
    public async Task Handle_ARankedBorrower_CarriesTheirName()
    {
        var result = await handler.Handle(new GetTopBorrowersQuery(From, To, Limit: 1), TestContext.Current.CancellationToken);

        Assert.Equal("Esther Vance", Assert.Single(result.Value).Name);
    }

    [Fact]
    public async Task Handle_ALimit_TakesThatMany()
    {
        var result = await handler.Handle(new GetTopBorrowersQuery(From, To, Limit: 2), TestContext.Current.CancellationToken);

        Assert.Equal([5, 2], result.Value.Select(borrower => borrower.BorrowerId));
    }

    [Fact]
    public async Task Handle_ARangeWithNoLoans_IsEmpty()
    {
        var query = new GetTopBorrowersQuery(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), Limit: null);

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        Assert.Empty(result.Value);
    }
}
