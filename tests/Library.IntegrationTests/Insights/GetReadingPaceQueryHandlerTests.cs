using Insights.Features.GetReadingPace;
using Library.Shared;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Insights;

public sealed class GetReadingPaceQueryHandlerTests : IClassFixture<TestDatabase>, IDisposable
{
    private readonly Readers readers;
    private readonly GetReadingPaceQueryHandler handler;

    public GetReadingPaceQueryHandlerTests(TestDatabase database)
    {
        readers = new Readers(database.ConnectionString);
        handler = new GetReadingPaceQueryHandler(readers.Loans, readers.Borrowers, readers.Books);
    }

    public void Dispose() => readers.Dispose();

    // Borrower 5 closed five loans: 3050 pages over 90 days, the same-day loan of book 1 counting
    // as one of them.
    [Fact]
    public async Task Handle_ABorrowerWithClosedLoans_DividesTheirPagesByTheirDays()
    {
        var result = await handler.Handle(new GetReadingPaceQuery(5), TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Value.ClosedLoans);
        Assert.Equal(33.89, Math.Round(result.Value.PagesPerDay, 2));
    }

    [Fact]
    public async Task Handle_ABorrowerWithClosedLoans_LeavesTheRoundingToTheEdge()
    {
        var result = await handler.Handle(new GetReadingPaceQuery(5), TestContext.Current.CancellationToken);

        Assert.Equal(3050d / 90, result.Value.PagesPerDay);
    }

    [Fact]
    public async Task Handle_ABorrowerWithOnlyOpenLoans_HasNoPaceYet()
    {
        var result = await handler.Handle(new GetReadingPaceQuery(7), TestContext.Current.CancellationToken);

        Assert.Equal(0, result.Value.ClosedLoans);
        Assert.Equal(0, result.Value.PagesPerDay);
    }

    [Fact]
    public async Task Handle_ABorrowerWithNoLoans_HasNoPaceYet()
    {
        var result = await handler.Handle(new GetReadingPaceQuery(8), TestContext.Current.CancellationToken);

        Assert.Equal(0, result.Value.ClosedLoans);
        Assert.Equal(0, result.Value.PagesPerDay);
    }

    [Fact]
    public async Task Handle_AnUnknownBorrower_IsNotFound()
    {
        var result = await handler.Handle(new GetReadingPaceQuery(99), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.NotFound, result.Error?.Kind);
    }
}
