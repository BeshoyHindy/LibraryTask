using Lending.Contracts;
using Library.Shared;
using Mediator;

namespace Lending.Features.ReturnBook;

public sealed record ReturnBookCommand(int LoanId, DateOnly ReturnedOn) : ICommand<Result<LoanDto>>;
