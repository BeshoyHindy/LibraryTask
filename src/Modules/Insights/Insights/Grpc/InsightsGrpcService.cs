using Grpc.Core;
using Insights.Contracts.Grpc;
using Insights.Features.GetAlsoBorrowedBooks;
using Insights.Features.GetMostBorrowedBooks;
using Insights.Features.GetReadingPace;
using Insights.Features.GetTopBorrowers;
using Library.Shared;
using Mediator;

namespace Insights.Grpc;

public sealed class InsightsGrpcService(IMediator mediator) : InsightsService.InsightsServiceBase
{
    public override async Task<GetMostBorrowedBooksResponse> GetMostBorrowedBooks(GetMostBorrowedBooksRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var query = new GetMostBorrowedBooksQuery(
            ValueOrThrow(ProtoDates.Optional(request.From, "from")),
            ValueOrThrow(ProtoDates.Optional(request.To, "to")),
            request.Limit);

        var books = ValueOrThrow(await mediator.Send(query, context.CancellationToken));

        var response = new GetMostBorrowedBooksResponse();
        response.Books.AddRange(books.Select(book => new MostBorrowedBook
        {
            BookId = book.BookId,
            Title = book.Title,
            Author = book.Author,
            LoanCount = book.Count,
        }));

        return response;
    }

    public override async Task<GetTopBorrowersResponse> GetTopBorrowers(GetTopBorrowersRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var query = new GetTopBorrowersQuery(
            ValueOrThrow(ProtoDates.Required(request.From, "from")),
            ValueOrThrow(ProtoDates.Required(request.To, "to")),
            request.Limit);

        var borrowers = ValueOrThrow(await mediator.Send(query, context.CancellationToken));

        var response = new GetTopBorrowersResponse();
        response.Borrowers.AddRange(borrowers.Select(borrower => new TopBorrower
        {
            BorrowerId = borrower.BorrowerId,
            Name = borrower.Name,
            LoanCount = borrower.LoanCount,
        }));

        return response;
    }

    public override async Task<GetReadingPaceResponse> GetReadingPace(GetReadingPaceRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var query = new GetReadingPaceQuery(request.BorrowerId);
        var pace = ValueOrThrow(await mediator.Send(query, context.CancellationToken));

        // The pace is a rate the handler keeps whole; two decimals is a presentation choice.
        return new GetReadingPaceResponse
        {
            BorrowerId = pace.BorrowerId,
            PagesPerDay = Math.Round(pace.PagesPerDay, 2),
            ClosedLoans = pace.ClosedLoans,
        };
    }

    public override async Task<GetAlsoBorrowedBooksResponse> GetAlsoBorrowedBooks(GetAlsoBorrowedBooksRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var query = new GetAlsoBorrowedBooksQuery(request.BookId, request.Limit);
        var books = ValueOrThrow(await mediator.Send(query, context.CancellationToken));

        var response = new GetAlsoBorrowedBooksResponse();
        response.Books.AddRange(books.Select(book => new AlsoBorrowedBook
        {
            BookId = book.BookId,
            Title = book.Title,
            Author = book.Author,
            BorrowerCount = book.Count,
        }));

        return response;
    }

    private static T ValueOrThrow<T>(Result<T> result)
    {
        if (result.Error is { } error)
        {
            throw error.ToRpcException();
        }

        return result.Value;
    }
}
