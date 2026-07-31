using LibraryManagementApi.Application.Reports.Queries.GetOverdueLoansReport;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetOverdueLoansReport;

public class GetOverdueLoansReportQueryValidatorTests
{
    private readonly GetOverdueLoansReportQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_HasNoErrors()
    {
        var result = _validator.Validate(new GetOverdueLoansReportQuery(null, 1, 20));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPageSizeOverLimit_HasError()
    {
        var result = _validator.Validate(new GetOverdueLoansReportQuery(null, 1, 101));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetOverdueLoansReportQuery.PageSize));
    }
}
