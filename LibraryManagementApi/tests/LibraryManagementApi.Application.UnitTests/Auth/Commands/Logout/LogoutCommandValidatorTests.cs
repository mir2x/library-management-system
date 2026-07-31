using LibraryManagementApi.Application.Auth.Commands.Logout;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Logout;

public class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new LogoutCommand("some-refresh-token-value");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyRefreshToken_HasError()
    {
        var command = new LogoutCommand("");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LogoutCommand.RefreshToken));
    }
}
