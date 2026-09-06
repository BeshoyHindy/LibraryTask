namespace Lending.Domain;

public sealed class Borrower
{
    public int Id { get; init; }

    public required string Name { get; init; }
}
