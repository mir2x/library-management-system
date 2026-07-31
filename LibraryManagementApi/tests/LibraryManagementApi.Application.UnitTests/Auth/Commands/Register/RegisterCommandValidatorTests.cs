using LibraryManagementApi.Application.Auth.Commands.Register;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new RegisterCommand("jane.doe@example.com", "Str0ngPass!", "Jane Doe");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasError(string email)
    {
        var command = new RegisterCommand(email, "Str0ngPass!", "Jane Doe");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Fact]
    public void Validate_WithEmptyFullName_HasError()
    {
        var command = new RegisterCommand("jane.doe@example.com", "Str0ngPass!", "");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.FullName));
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("NoDigitsHere")]
    public void Validate_WithWeakPassword_HasError(string password)
    {
        var command = new RegisterCommand("jane.doe@example.com", password, "Jane Doe");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Password));
    }
}
