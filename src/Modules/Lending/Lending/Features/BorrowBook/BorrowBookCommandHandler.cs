using Catalog.Contracts;
using Lending.Contracts;
using Lending.Data;
using Lending.Domain;
using Library.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Lending.Features.BorrowBook;

public sealed class BorrowBookCommandHandler(
    LendingDbContext dbContext,
    IBookReader books,
    ILogger<BorrowBookCommandHandler> logger) : ICommandHandler<BorrowBookCommand, Result<LoanDto>>
{
    public async ValueTask<Result<LoanDto>> Handle(BorrowBookCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!await books.ExistsAsync(command.BookId, cancellationToken))
        {
            return Error.NotFound($"Book {command.BookId} was not found.");
        }

        if (!await dbContext.Borrowers.AnyAsync(borrower => borrower.Id == command.BorrowerId, cancellationToken))
        {
            return Error.NotFound($"Borrower {command.BorrowerId} was not found.");
        }

        if (await dbContext.Loans.AnyAsync(loan => loan.BookId == command.BookId && loan.ReturnedOn == null, cancellationToken))
        {
            return Error.Conflict($"Book {command.BookId} is already on loan.");
        }

        var loan = Loan.Open(command.BookId, command.BorrowerId, command.BorrowedOn);
        dbContext.Loans.Add(loan);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Two borrow requests for the same book passed the check above at the same time; the
            // partial unique index on open loans let only one of them in.
            return Error.Conflict($"Book {command.BookId} is already on loan.");
        }

        logger.LogInformation(
            "Loan {LoanId} opened for book {BookId} and borrower {BorrowerId}",
            loan.Id,
            loan.BookId,
            loan.BorrowerId);

        return loan.ToDto();
    }
}
