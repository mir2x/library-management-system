using LibraryManagementApi.Application.Reports.Queries.GetMemberActivityReport;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetMemberActivityReport;

public class GetMemberActivityReportQueryValidatorTests
{
    private readonly GetMemberActivityReportQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_HasNoErrors()
    {
        var result = _validator.Validate(new GetMemberActivityReportQuery(null, 1, 20));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPageNumberBelowOne_HasError()
    {
        var result = _validator.Validate(new GetMemberActivityReportQuery(null, 0, 20));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetMemberActivityReportQuery.PageNumber));
    }
}
