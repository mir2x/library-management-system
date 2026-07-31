using LibraryManagementApi.Application.Books.Commands.CreateBook;

namespace LibraryManagementApi.Application.UnitTests.Books.Commands.CreateBook;

public class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator = new();

    private static CreateBookCommand ValidCommand() =>
        new("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, "A handbook of agile software craftsmanship.");

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyTitle_HasError()
    {
        var command = ValidCommand() with { Title = "" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBookCommand.Title));
    }

    [Fact]
    public void Validate_WithInvalidIsbn_HasError()
    {
        var command = ValidCommand() with { Isbn = "not-an-isbn" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBookCommand.Isbn));
    }

    [Theory]
    [InlineData(1449)]
    [InlineData(3000)]
    public void Validate_WithPublishedYearOutOfRange_HasError(int year)
    {
        var command = ValidCommand() with { PublishedYear = year };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBookCommand.PublishedYear));
    }
}
