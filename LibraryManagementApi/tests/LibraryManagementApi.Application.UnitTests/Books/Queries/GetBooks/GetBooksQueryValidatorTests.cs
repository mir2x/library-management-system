using LibraryManagementApi.Application.Books.Queries.GetBooks;

namespace LibraryManagementApi.Application.UnitTests.Books.Queries.GetBooks;

public class GetBooksQueryValidatorTests
{
    private readonly GetBooksQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaultPaging_HasNoErrors()
    {
        var query = new GetBooksQuery(null);

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPageNumberLessThanOne_HasError()
    {
        var query = new GetBooksQuery(null, PageNumber: 0);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetBooksQuery.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithPageSizeOutOfRange_HasError(int pageSize)
    {
        var query = new GetBooksQuery(null, PageSize: pageSize);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetBooksQuery.PageSize));
    }
}
