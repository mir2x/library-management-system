using LibraryManagementApi.Application.Branches.Commands.CreateBranch;

namespace LibraryManagementApi.Application.UnitTests.Branches.Commands.CreateBranch;

public class CreateBranchCommandValidatorTests
{
    private readonly CreateBranchCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new CreateBranchCommand("Downtown Branch", "123 Main St", "555-0100", "downtown@library.org");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithValidCommandAndNoOptionalFields_HasNoErrors()
    {
        var command = new CreateBranchCommand("Downtown Branch", "123 Main St", null, null);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_HasError()
    {
        var command = new CreateBranchCommand("", "123 Main St", null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Name));
    }

    [Fact]
    public void Validate_WithEmptyAddress_HasError()
    {
        var command = new CreateBranchCommand("Downtown Branch", "", null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Address));
    }

    [Fact]
    public void Validate_WithInvalidEmail_HasError()
    {
        var command = new CreateBranchCommand("Downtown Branch", "123 Main St", null, "not-an-email");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Email));
    }
}
