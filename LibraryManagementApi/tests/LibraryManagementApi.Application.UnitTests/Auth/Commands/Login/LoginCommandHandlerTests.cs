using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Auth.Commands.Login;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Constants;
using NSubstitute;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuthTokenIssuer _authTokenIssuer = Substitute.For<IAuthTokenIssuer>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_identityService, _authTokenIssuer);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokens()
    {
        var command = new LoginCommand("jane.doe@example.com", "Str0ngPass!");
        var authenticatedUser = new AuthenticatedUser("user-1", command.Email, "Jane Doe", [Roles.Member]);
        var expectedResponse = new AuthResponse(
            authenticatedUser.Id,
            authenticatedUser.Email,
            authenticatedUser.FullName,
            authenticatedUser.Roles,
            "access-token",
            DateTime.UtcNow.AddMinutes(15),
            "refresh-token",
            DateTime.UtcNow.AddDays(7));

        _identityService
            .ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(authenticatedUser);

        _authTokenIssuer
            .IssueTokensAsync(authenticatedUser, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(expectedResponse, result.Value);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ReturnsFailureAndDoesNotIssueTokens()
    {
        var command = new LoginCommand("jane.doe@example.com", "wrong-password");

        _identityService
            .ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns((AuthenticatedUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Invalid email or password."], result.Errors);
        await _authTokenIssuer.DidNotReceive().IssueTokensAsync(Arg.Any<AuthenticatedUser>(), Arg.Any<CancellationToken>());
    }
}
