using Lending.Contracts;
using Library.Shared;
using Mediator;

namespace Lending.Features.BorrowBook;

public sealed record BorrowBookCommand(int BookId, int BorrowerId, DateOnly BorrowedOn) : ICommand<Result<LoanDto>>;
