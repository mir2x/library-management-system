using LibraryManagementApi.Application.Members.Commands.UpdateMember;

namespace LibraryManagementApi.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidatorTests
{
    private readonly UpdateMemberCommandValidator _validator = new();

    [Fact]
    public void Validate_WithAllFieldsOmitted_HasNoErrors()
    {
        var command = new UpdateMemberCommand(Guid.NewGuid(), null, null, null, null, null);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyFullNameProvided_HasError()
    {
        var command = new UpdateMemberCommand(Guid.NewGuid(), "", null, null, null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateMemberCommand.FullName));
    }

    [Fact]
    public void Validate_WithInvalidEmailProvided_HasError()
    {
        var command = new UpdateMemberCommand(Guid.NewGuid(), null, "not-an-email", null, null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateMemberCommand.Email));
    }

    [Fact]
    public void Validate_WithEmptyHomeBranchIdProvided_HasError()
    {
        var command = new UpdateMemberCommand(Guid.NewGuid(), null, null, null, null, Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateMemberCommand.HomeBranchId));
    }
}
