using Insights.Features.GetMostBorrowedBooks;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Insights;

public sealed class GetMostBorrowedBooksQueryHandlerTests : IClassFixture<TestDatabase>, IDisposable
{
    private readonly Readers readers;
    private readonly GetMostBorrowedBooksQueryHandler handler;

    public GetMostBorrowedBooksQueryHandlerTests(TestDatabase database)
    {
        readers = new Readers(database.ConnectionString);
        handler = new GetMostBorrowedBooksQueryHandler(readers.Loans, readers.Books);
    }

    public void Dispose() => readers.Dispose();

    [Fact]
    public async Task Handle_WithNoRange_RanksTheTenMostBorrowedBooks()
    {
        var result = await handler.Handle(Query(limit: null), TestContext.Current.CancellationToken);

        Assert.Equal([7, 2, 4, 1, 10, 3, 12, 6, 9, 5], result.Value.Select(book => book.BookId));
        Assert.Equal([5, 5, 5, 5, 4, 3, 3, 3, 3, 2], result.Value.Select(book => book.Count));
    }

    // Books 7, 2, 4 and 1 all have five loans, so only the title in ordinal order tells them
    // apart: "Meridian Nine", "Salt and Longitude", "The Glassblower's Apprentice", "The Silent
    // Cartographer".
    [Fact]
    public async Task Handle_BooksWithTheSameCount_AreOrderedByTitle()
    {
        var result = await handler.Handle(Query(limit: 4), TestContext.Current.CancellationToken);

        Assert.Equal(
            ["Meridian Nine", "Salt and Longitude", "The Glassblower's Apprentice", "The Silent Cartographer"],
            result.Value.Select(book => book.Title));
    }

    [Fact]
    public async Task Handle_ALimit_TakesThatMany()
    {
        var result = await handler.Handle(Query(limit: 3), TestContext.Current.CancellationToken);

        Assert.Equal([7, 2, 4], result.Value.Select(book => book.BookId));
    }

    [Fact]
    public async Task Handle_ALimitOfZero_TakesTheDefaultTen()
    {
        var result = await handler.Handle(Query(limit: 0), TestContext.Current.CancellationToken);

        Assert.Equal(10, result.Value.Count);
    }

    [Fact]
    public async Task Handle_ARange_CountsOnlyLoansBorrowedInIt()
    {
        var query = new GetMostBorrowedBooksQuery(new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31), Limit: null);

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        Assert.Equal([7, 4, 1, 12, 10, 5, 2, 6, 9], result.Value.Select(book => book.BookId));
        Assert.Equal([2, 2, 2, 1, 1, 1, 1, 1, 1], result.Value.Select(book => book.Count));
    }

    [Fact]
    public async Task Handle_ARangeWithNoLoans_IsEmpty()
    {
        var query = new GetMostBorrowedBooksQuery(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), Limit: null);

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_ARankedBook_CarriesItsAuthor()
    {
        var result = await handler.Handle(Query(limit: 1), TestContext.Current.CancellationToken);

        Assert.Equal("Sofia Marchetti", Assert.Single(result.Value).Author);
    }

    private static GetMostBorrowedBooksQuery Query(int? limit) => new(From: null, To: null, limit);
}
