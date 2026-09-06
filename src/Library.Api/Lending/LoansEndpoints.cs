using Lending.Contracts.Grpc;
using LoanMessage = Lending.Contracts.Grpc.Loan;

namespace Library.Api.Lending;

public static class LoansEndpoints
{
    public static void MapLoansEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var loans = endpoints.MapGroup("/api/loans").WithTags("Loans");

        loans.MapPost("/", async (
            BorrowLoanRequest body,
            LendingService.LendingServiceClient lending,
            CancellationToken cancellationToken) =>
        {
            var request = new BorrowBookRequest
            {
                BookId = body.BookId,
                BorrowerId = body.BorrowerId,
                BorrowedOn = ApiDates.ToDate(body.BorrowedOn),
            };

            var loan = await lending.BorrowBookAsync(request, cancellationToken: cancellationToken);

            return TypedResults.Created($"/api/loans/{loan.LoanId}", ToResponse(loan));
        }).WithSummary("Borrow a book. Fails with 409 when the book is already out.");

        loans.MapPost("/{loanId:int}/return", async (
            int loanId,
            ReturnLoanRequest body,
            LendingService.LendingServiceClient lending,
            CancellationToken cancellationToken) =>
        {
            var request = new ReturnBookRequest { LoanId = loanId, ReturnedOn = ApiDates.ToDate(body.ReturnedOn) };

            var loan = await lending.ReturnBookAsync(request, cancellationToken: cancellationToken);

            return ToResponse(loan);
        }).WithSummary("Return a loan. Fails with 409 when it is already closed.");

        loans.MapGet("/{loanId:int}", async (
            int loanId,
            LendingService.LendingServiceClient lending,
            CancellationToken cancellationToken) =>
        {
            var loan = await lending.GetLoanAsync(
                new GetLoanRequest { LoanId = loanId },
                cancellationToken: cancellationToken);

            return ToResponse(loan);
        }).WithSummary("Get one loan by id.");
    }

    private static LoanResponse ToResponse(LoanMessage loan) =>
        new(
            loan.LoanId,
            loan.BookId,
            loan.BorrowerId,
            ApiDates.ToDateOnly(loan.BorrowedOn),
            loan.ReturnedOn is { } returnedOn ? ApiDates.ToDateOnly(returnedOn) : null);
}
