using Library.Shared;
using Mediator;

namespace Insights.Features.GetAlsoBorrowedBooks;

public sealed record GetAlsoBorrowedBooksQuery(int BookId, int? Limit)
    : IQuery<Result<IReadOnlyList<RankedBook>>>;
