using LibraryManagementApi.Application.Members.Commands.CreateMember;

namespace LibraryManagementApi.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandValidatorTests
{
    private readonly CreateMemberCommandValidator _validator = new();

    private static CreateMemberCommand ValidCommand() =>
        new("Jane Doe", "jane.doe@example.com", "555-0100", "123 Main St", Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyFullName_HasError()
    {
        var command = ValidCommand() with { FullName = "" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMemberCommand.FullName));
    }

    [Fact]
    public void Validate_WithInvalidEmail_HasError()
    {
        var command = ValidCommand() with { Email = "not-an-email" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMemberCommand.Email));
    }

    [Fact]
    public void Validate_WithEmptyHomeBranchId_HasError()
    {
        var command = ValidCommand() with { HomeBranchId = Guid.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMemberCommand.HomeBranchId));
    }
}
