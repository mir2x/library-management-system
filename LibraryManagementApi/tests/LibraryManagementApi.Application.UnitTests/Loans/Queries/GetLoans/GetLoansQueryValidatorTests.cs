using LibraryManagementApi.Application.Loans.Queries.GetLoans;

namespace LibraryManagementApi.Application.UnitTests.Loans.Queries.GetLoans;

public class GetLoansQueryValidatorTests
{
    private readonly GetLoansQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaultPaging_HasNoErrors()
    {
        var query = new GetLoansQuery(null, null, null, null);

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPageNumberLessThanOne_HasError()
    {
        var query = new GetLoansQuery(null, null, null, null, PageNumber: 0);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetLoansQuery.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithPageSizeOutOfRange_HasError(int pageSize)
    {
        var query = new GetLoansQuery(null, null, null, null, PageSize: pageSize);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetLoansQuery.PageSize));
    }
}
