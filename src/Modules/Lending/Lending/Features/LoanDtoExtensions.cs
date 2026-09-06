using Lending.Contracts;
using Lending.Domain;

namespace Lending.Features;

internal static class LoanDtoExtensions
{
    public static LoanDto ToDto(this Loan loan) =>
        new(loan.Id, loan.BookId, loan.BorrowerId, loan.BorrowedOn, loan.ReturnedOn);
}
