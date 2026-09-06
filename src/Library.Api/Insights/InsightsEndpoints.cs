using Insights.Contracts.Grpc;

namespace Library.Api.Insights;

public static class InsightsEndpoints
{
    public static void MapInsightsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var insights = endpoints.MapGroup("/api/insights").WithTags("Insights");

        insights.MapGet("/most-borrowed-books", async (
            DateOnly? from,
            DateOnly? to,
            int? limit,
            InsightsService.InsightsServiceClient insights,
            CancellationToken cancellationToken) =>
        {
            // An absent limit stays absent: 0 is the proto's way of leaving it out, and the
            // default of 10 is the service's rule rather than the Api's.
            var request = new GetMostBorrowedBooksRequest
            {
                From = ApiDates.ToDateOrNull(from),
                To = ApiDates.ToDateOrNull(to),
                Limit = limit ?? 0,
            };

            var response = await insights.GetMostBorrowedBooksAsync(request, cancellationToken: cancellationToken);

            return response.Books
                .Select(book => new MostBorrowedBookResponse(book.BookId, book.Title, book.Author, book.LoanCount))
                .ToArray();
        }).WithSummary("The most borrowed books, optionally within an inclusive borrow-date range.");

        insights.MapGet("/top-borrowers", async (
            DateOnly? from,
            DateOnly? to,
            int? limit,
            InsightsService.InsightsServiceClient insights,
            CancellationToken cancellationToken) =>
        {
            var request = new GetTopBorrowersRequest
            {
                From = ApiDates.ToDateOrNull(from),
                To = ApiDates.ToDateOrNull(to),
                Limit = limit ?? 0,
            };

            var response = await insights.GetTopBorrowersAsync(request, cancellationToken: cancellationToken);

            return response.Borrowers
                .Select(borrower => new TopBorrowerResponse(borrower.BorrowerId, borrower.Name, borrower.LoanCount))
                .ToArray();
        }).WithSummary("The borrowers with the most loans in a required borrow-date range.");

        insights.MapGet("/borrowers/{borrowerId:int}/reading-pace", async (
            int borrowerId,
            InsightsService.InsightsServiceClient insights,
            CancellationToken cancellationToken) =>
        {
            var response = await insights.GetReadingPaceAsync(
                new GetReadingPaceRequest { BorrowerId = borrowerId },
                cancellationToken: cancellationToken);

            return new ReadingPaceResponse(response.BorrowerId, response.PagesPerDay, response.ClosedLoans);
        }).WithSummary("A borrower's pages per day over their returned loans.");

        insights.MapGet("/books/{bookId:int}/also-borrowed", async (
            int bookId,
            int? limit,
            InsightsService.InsightsServiceClient insights,
            CancellationToken cancellationToken) =>
        {
            var request = new GetAlsoBorrowedBooksRequest { BookId = bookId, Limit = limit ?? 0 };

            var response = await insights.GetAlsoBorrowedBooksAsync(request, cancellationToken: cancellationToken);

            return response.Books
                .Select(book => new AlsoBorrowedBookResponse(book.BookId, book.Title, book.Author, book.BorrowerCount))
                .ToArray();
        }).WithSummary("Other books borrowed by the people who borrowed this one.");
    }
}
