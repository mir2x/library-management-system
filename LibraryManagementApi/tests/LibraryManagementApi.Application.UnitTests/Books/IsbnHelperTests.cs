using LibraryManagementApi.Application.Books;

namespace LibraryManagementApi.Application.UnitTests.Books;

public class IsbnHelperTests
{
    [Theory]
    [InlineData("9780132350884")]
    [InlineData("978-0-13-235088-4")]
    [InlineData("0132350882")]
    [InlineData("013235088X")]
    [InlineData("0-13-235088-X")]
    public void IsValid_WithWellFormedIsbn_ReturnsTrue(string isbn)
    {
        Assert.True(IsbnHelper.IsValid(isbn));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("not-an-isbn")]
    [InlineData("978013235088")]
    public void IsValid_WithMalformedIsbn_ReturnsFalse(string isbn)
    {
        Assert.False(IsbnHelper.IsValid(isbn));
    }

    [Fact]
    public void Normalize_RemovesHyphensSpacesAndUppercases()
    {
        var result = IsbnHelper.Normalize("0-13-235088-x");

        Assert.Equal("013235088X", result);
    }
}
