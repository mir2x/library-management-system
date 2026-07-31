using LibraryManagementApi.Application.Books.Commands.UpdateBook;

namespace LibraryManagementApi.Application.UnitTests.Books.Commands.UpdateBook;

public class UpdateBookCommandValidatorTests
{
    private readonly UpdateBookCommandValidator _validator = new();

    [Fact]
    public void Validate_WithAllFieldsOmitted_HasNoErrors()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), null, null, null, null, null);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyTitleProvided_HasError()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), "", null, null, null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBookCommand.Title));
    }

    [Fact]
    public void Validate_WithPublishedYearOutOfRange_HasError()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), null, null, null, 3000, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBookCommand.PublishedYear));
    }
}
