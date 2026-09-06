using Catalog.Contracts;
using Library.Shared;
using Mediator;

namespace Catalog.Features.ListBooks;

// The catalogue is twelve rows and is not paginated, so the query carries nothing and needs no
// validator.
public sealed record ListBooksQuery : IQuery<Result<IReadOnlyList<BookSummary>>>;
