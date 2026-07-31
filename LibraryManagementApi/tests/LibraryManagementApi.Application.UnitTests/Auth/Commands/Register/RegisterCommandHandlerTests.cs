using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Auth.Commands.Register;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Constants;
using NSubstitute;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuthTokenIssuer _authTokenIssuer = Substitute.For<IAuthTokenIssuer>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_identityService, _authTokenIssuer);
    }

    [Fact]
    public async Task Handle_WithNewEmail_CreatesUserAsMemberAndReturnsTokens()
    {
        var command = new RegisterCommand("jane.doe@example.com", "Str0ngPass!", "Jane Doe");
        var authenticatedUser = new AuthenticatedUser("user-1", command.Email, command.FullName, [Roles.Member]);
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
            .CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Member, Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticatedUser>.Success(authenticatedUser));

        _authTokenIssuer
            .IssueTokensAsync(authenticatedUser, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(expectedResponse, result.Value);
        await _identityService.Received(1)
            .CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Member, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceFails_ReturnsFailureAndDoesNotIssueTokens()
    {
        var command = new RegisterCommand("jane.doe@example.com", "Str0ngPass!", "Jane Doe");
        string[] errors = ["Email 'jane.doe@example.com' is already taken."];

        _identityService
            .CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Member, Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticatedUser>.Failure(errors));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(errors, result.Errors);
        await _authTokenIssuer.DidNotReceive().IssueTokensAsync(Arg.Any<AuthenticatedUser>(), Arg.Any<CancellationToken>());
    }
}
