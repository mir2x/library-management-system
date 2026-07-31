using LibraryManagementApi.Application.Auth.Commands.ResetPassword;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new ResetPasswordCommand("jane.doe@example.com", "reset-token", "Str0ngPass!");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasError(string email)
    {
        var command = new ResetPasswordCommand(email, "reset-token", "Str0ngPass!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.Email));
    }

    [Fact]
    public void Validate_WithEmptyToken_HasError()
    {
        var command = new ResetPasswordCommand("jane.doe@example.com", "", "Str0ngPass!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.Token));
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("NoDigitsHere")]
    public void Validate_WithWeakNewPassword_HasError(string newPassword)
    {
        var command = new ResetPasswordCommand("jane.doe@example.com", "reset-token", newPassword);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }
}
