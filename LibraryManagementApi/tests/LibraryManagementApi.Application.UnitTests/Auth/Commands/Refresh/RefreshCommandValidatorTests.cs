using LibraryManagementApi.Application.Auth.Commands.Refresh;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Refresh;

public class RefreshCommandValidatorTests
{
    private readonly RefreshCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new RefreshCommand("some-refresh-token-value");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyRefreshToken_HasError()
    {
        var command = new RefreshCommand("");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshCommand.RefreshToken));
    }
}
