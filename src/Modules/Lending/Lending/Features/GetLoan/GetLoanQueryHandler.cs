using Lending.Contracts;
using Lending.Data;
using Library.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Lending.Features.GetLoan;

public sealed class GetLoanQueryHandler(LendingDbContext dbContext) : IQueryHandler<GetLoanQuery, Result<LoanDto>>
{
    public async ValueTask<Result<LoanDto>> Handle(GetLoanQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var loan = await dbContext.Loans
            .AsNoTracking()
            .Where(loan => loan.Id == query.LoanId)
            .Select(loan => new LoanDto(loan.Id, loan.BookId, loan.BorrowerId, loan.BorrowedOn, loan.ReturnedOn))
            .FirstOrDefaultAsync(cancellationToken);

        return loan is null
            ? Error.NotFound($"Loan {query.LoanId} was not found.")
            : loan;
    }
}
