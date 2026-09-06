using Lending.Features.GetLoan;
using Xunit;

namespace Library.UnitTests.Lending;

public sealed class GetLoanQueryValidatorTests
{
    private readonly GetLoanQueryValidator validator = new();

    [Fact]
    public void Validate_APositiveLoanId_IsValid()
    {
        var result = validator.Validate(new GetLoanQuery(41));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ALoanIdThatIsNotPositive_FailsOnLoanId(int loanId)
    {
        var result = validator.Validate(new GetLoanQuery(loanId));

        Assert.Equal("LoanId", Assert.Single(result.Errors).PropertyName);
    }
}
