using Catalog.Contracts.Grpc;
using Catalog.Features.ListBooks;
using Grpc.Core;
using Library.Shared;
using Mediator;
using BookMessage = Catalog.Contracts.Grpc.Book;

namespace Catalog.Grpc;

public sealed class CatalogGrpcService(IMediator mediator) : CatalogService.CatalogServiceBase
{
    public override async Task<ListBooksResponse> ListBooks(ListBooksRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = await mediator.Send(new ListBooksQuery(), context.CancellationToken);
        if (result.Error is { } error)
        {
            throw error.ToRpcException();
        }

        var response = new ListBooksResponse();
        response.Books.AddRange(result.Value.Select(book => new BookMessage
        {
            BookId = book.Id,
            Title = book.Title,
            Author = book.Author,
            Pages = book.Pages,
        }));

        return response;
    }
}
