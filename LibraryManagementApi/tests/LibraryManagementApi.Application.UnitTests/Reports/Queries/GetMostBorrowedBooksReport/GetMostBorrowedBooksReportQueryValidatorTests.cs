using LibraryManagementApi.Application.Reports.Queries.GetMostBorrowedBooksReport;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetMostBorrowedBooksReport;

public class GetMostBorrowedBooksReportQueryValidatorTests
{
    private readonly GetMostBorrowedBooksReportQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_HasNoErrors()
    {
        var result = _validator.Validate(new GetMostBorrowedBooksReportQuery(null, null, null, 10));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithTopOutOfRange_HasError()
    {
        var result = _validator.Validate(new GetMostBorrowedBooksReportQuery(null, null, null, 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetMostBorrowedBooksReportQuery.Top));
    }

    [Fact]
    public void Validate_WithToUtcBeforeFromUtc_HasError()
    {
        var now = DateTime.UtcNow;
        var result = _validator.Validate(new GetMostBorrowedBooksReportQuery(null, now, now.AddDays(-1), 10));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetMostBorrowedBooksReportQuery.ToUtc));
    }
}
