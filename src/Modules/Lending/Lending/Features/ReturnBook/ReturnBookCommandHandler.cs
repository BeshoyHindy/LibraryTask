using Lending.Contracts;
using Lending.Data;
using Library.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lending.Features.ReturnBook;

public sealed class ReturnBookCommandHandler(
    LendingDbContext dbContext,
    ILogger<ReturnBookCommandHandler> logger) : ICommandHandler<ReturnBookCommand, Result<LoanDto>>
{
    public async ValueTask<Result<LoanDto>> Handle(ReturnBookCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loan = await dbContext.Loans.FirstOrDefaultAsync(loan => loan.Id == command.LoanId, cancellationToken);
        if (loan is null)
        {
            return Error.NotFound($"Loan {command.LoanId} was not found.");
        }

        var returned = loan.Return(command.ReturnedOn);
        if (returned.Error is { } error)
        {
            return error;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Loan {LoanId} returned on {ReturnedOn}", loan.Id, command.ReturnedOn);

        return loan.ToDto();
    }
}
