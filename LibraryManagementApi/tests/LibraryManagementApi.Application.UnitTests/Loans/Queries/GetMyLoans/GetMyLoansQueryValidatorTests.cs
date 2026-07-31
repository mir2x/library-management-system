using LibraryManagementApi.Application.Loans.Queries.GetMyLoans;

namespace LibraryManagementApi.Application.UnitTests.Loans.Queries.GetMyLoans;

public class GetMyLoansQueryValidatorTests
{
    private readonly GetMyLoansQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaultPaging_HasNoErrors()
    {
        var query = new GetMyLoansQuery("user-1");

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPageNumberLessThanOne_HasError()
    {
        var query = new GetMyLoansQuery("user-1", PageNumber: 0);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetMyLoansQuery.PageNumber));
    }
}
