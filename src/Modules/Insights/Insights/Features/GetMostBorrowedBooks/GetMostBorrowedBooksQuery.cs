using Library.Shared;
using Mediator;

namespace Insights.Features.GetMostBorrowedBooks;

public sealed record GetMostBorrowedBooksQuery(DateOnly? From, DateOnly? To, int? Limit)
    : IQuery<Result<IReadOnlyList<RankedBook>>>;
