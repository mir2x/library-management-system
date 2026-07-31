using LibraryManagementApi.Application.Auth.Commands.Login;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new LoginCommand("jane.doe@example.com", "whatever-password");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasError(string email)
    {
        var command = new LoginCommand(email, "whatever-password");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_WithEmptyPassword_HasError()
    {
        var command = new LoginCommand("jane.doe@example.com", "");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
