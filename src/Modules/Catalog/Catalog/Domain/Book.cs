namespace Catalog.Domain;

public sealed class Book
{
    public int Id { get; init; }

    public required string Title { get; init; }

    public required string Author { get; init; }

    public int Pages { get; init; }
}
