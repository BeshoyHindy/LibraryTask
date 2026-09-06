using Insights.Features;
using Insights.Features.GetAlsoBorrowedBooks;
using Library.Shared;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Insights;

public sealed class GetAlsoBorrowedBooksQueryHandlerTests : IClassFixture<TestDatabase>, IDisposable
{
    private readonly Readers readers;
    private readonly GetAlsoBorrowedBooksQueryHandler handler;

    public GetAlsoBorrowedBooksQueryHandlerTests(TestDatabase database)
    {
        readers = new Readers(database.ConnectionString);
        handler = new GetAlsoBorrowedBooksQueryHandler(readers.Loans, readers.Books);
    }

    public void Dispose() => readers.Dispose();

    // Books 7, 2 and 1 were each taken by four of the five borrowers of book 4, so the title in
    // ordinal order breaks the tie; book 4 itself is not in its own ranking.
    [Fact]
    public async Task Handle_ABorrowedBook_RanksTheOtherBooksItsBorrowersTook()
    {
        var result = await handler.Handle(new GetAlsoBorrowedBooksQuery(4, Limit: null), TestContext.Current.CancellationToken);

        Assert.Equal([7, 2, 1, 12, 10, 3, 5, 6, 9], result.Value.Select(book => book.BookId));
        Assert.Equal([4, 4, 4, 3, 3, 2, 2, 2, 2], result.Value.Select(book => book.Count));
    }

    [Fact]
    public async Task Handle_ALimit_TakesThatMany()
    {
        var result = await handler.Handle(new GetAlsoBorrowedBooksQuery(4, Limit: 2), TestContext.Current.CancellationToken);

        Assert.Equal([7, 2], result.Value.Select(book => book.BookId));
    }

    // Book 11 has one borrower, 7, whose only other book is the one they still have out on loan,
    // so an open loan counts towards the ranking as a closed one does.
    [Fact]
    public async Task Handle_ABookWithOneBorrower_RanksOnlyTheBooksThatBorrowerTook()
    {
        var result = await handler.Handle(new GetAlsoBorrowedBooksQuery(11, Limit: null), TestContext.Current.CancellationToken);

        Assert.Equal(new RankedBook(3, "A Short History of Rain", "Priya Raman", 1), Assert.Single(result.Value));
    }

    [Fact]
    public async Task Handle_AnUnknownBook_IsNotFound()
    {
        var result = await handler.Handle(new GetAlsoBorrowedBooksQuery(99, Limit: null), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.NotFound, result.Error?.Kind);
    }
}
