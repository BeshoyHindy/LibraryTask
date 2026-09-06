using FluentValidation;
using Library.Shared;
using Mediator;
using Xunit;

namespace Library.UnitTests.Shared;

public sealed class ValidationBehaviourTests
{
    [Fact]
    public async Task Handle_WithNoValidators_CallsTheHandler()
    {
        var behaviour = new ValidationBehaviour<CountBooksQuery, Result<int>>([]);

        var result = await behaviour.Handle(new CountBooksQuery(0), Handler, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value);
    }

    [Fact]
    public async Task Handle_WhenTheMessageIsValid_CallsTheHandler()
    {
        var behaviour = new ValidationBehaviour<CountBooksQuery, Result<int>>([new CountBooksQueryValidator()]);

        var result = await behaviour.Handle(new CountBooksQuery(10), Handler, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenTheMessageIsInvalid_ReturnsAValidationFailure()
    {
        var behaviour = new ValidationBehaviour<CountBooksQuery, Result<int>>([new CountBooksQueryValidator()]);

        var result = await behaviour.Handle(new CountBooksQuery(0), NeverCalled, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Error?.Kind);
    }

    [Fact]
    public async Task Handle_WhenTheMessageIsInvalid_NamesTheFieldAsTheCallerSentIt()
    {
        var behaviour = new ValidationBehaviour<CountBooksQuery, Result<int>>([new CountBooksQueryValidator()]);

        var result = await behaviour.Handle(new CountBooksQuery(0), NeverCalled, TestContext.Current.CancellationToken);

        var field = Assert.Single(result.Error?.Fields ?? []);
        Assert.Equal("limit", field.Field);
    }

    private static ValueTask<Result<int>> Handler(CountBooksQuery query, CancellationToken cancellationToken) =>
        ValueTask.FromResult<Result<int>>(12);

    private static ValueTask<Result<int>> NeverCalled(CountBooksQuery query, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("The handler ran despite an invalid message.");

    private sealed record CountBooksQuery(int Limit) : IQuery<Result<int>>;

    private sealed class CountBooksQueryValidator : AbstractValidator<CountBooksQuery>
    {
        public CountBooksQueryValidator() => RuleFor(query => query.Limit).GreaterThan(0);
    }
}
