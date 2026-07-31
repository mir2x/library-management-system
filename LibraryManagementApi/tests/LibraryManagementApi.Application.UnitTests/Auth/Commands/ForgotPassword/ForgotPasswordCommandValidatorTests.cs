using LibraryManagementApi.Application.Auth.Commands.ForgotPassword;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new ForgotPasswordCommand("jane.doe@example.com");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasError(string email)
    {
        var command = new ForgotPasswordCommand(email);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
    }
}
