using Lending.Contracts;
using Library.Shared;
using Mediator;

namespace Lending.Features.GetLoan;

public sealed record GetLoanQuery(int LoanId) : IQuery<Result<LoanDto>>;
