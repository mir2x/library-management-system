using LibraryManagementApi.Application.Branches.Commands.UpdateBranch;

namespace LibraryManagementApi.Application.UnitTests.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandValidatorTests
{
    private readonly UpdateBranchCommandValidator _validator = new();

    [Fact]
    public void Validate_WithAllFieldsOmitted_HasNoErrors()
    {
        var command = new UpdateBranchCommand(Guid.NewGuid(), null, null, null, null);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyNameProvided_HasError()
    {
        var command = new UpdateBranchCommand(Guid.NewGuid(), "", null, null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBranchCommand.Name));
    }

    [Fact]
    public void Validate_WithInvalidEmailProvided_HasError()
    {
        var command = new UpdateBranchCommand(Guid.NewGuid(), null, null, null, "not-an-email");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBranchCommand.Email));
    }
}
