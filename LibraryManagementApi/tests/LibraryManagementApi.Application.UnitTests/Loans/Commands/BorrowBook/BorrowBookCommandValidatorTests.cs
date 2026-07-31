using LibraryManagementApi.Application.Loans.Commands.BorrowBook;

namespace LibraryManagementApi.Application.UnitTests.Loans.Commands.BorrowBook;

public class BorrowBookCommandValidatorTests
{
    private readonly BorrowBookCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new BorrowBookCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyMemberId_HasError()
    {
        var command = new BorrowBookCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(BorrowBookCommand.MemberId));
    }

    [Fact]
    public void Validate_WithEmptyBookId_HasError()
    {
        var command = new BorrowBookCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(BorrowBookCommand.BookId));
    }

    [Fact]
    public void Validate_WithEmptyBranchId_HasError()
    {
        var command = new BorrowBookCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(BorrowBookCommand.BranchId));
    }
}
