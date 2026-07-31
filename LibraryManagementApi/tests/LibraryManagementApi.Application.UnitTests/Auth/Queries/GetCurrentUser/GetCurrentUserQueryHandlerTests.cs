using LibraryManagementApi.Application.Auth.Queries.GetCurrentUser;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Constants;
using NSubstitute;

namespace LibraryManagementApi.Application.UnitTests.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _handler = new GetCurrentUserQueryHandler(_identityService);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ReturnsCurrentUser()
    {
        var user = new AuthenticatedUser("user-1", "jane.doe@example.com", "Jane Doe", [Roles.Member]);
        _identityService.GetUserByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new GetCurrentUserQuery(user.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(user.Id, result.Value!.UserId);
        Assert.Equal(user.Email, result.Value!.Email);
        Assert.Equal(user.FullName, result.Value!.FullName);
        Assert.Equal(user.Roles, result.Value!.Roles);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ReturnsFailure()
    {
        _identityService.GetUserByIdAsync("missing-user", Arg.Any<CancellationToken>()).Returns((AuthenticatedUser?)null);

        var result = await _handler.Handle(new GetCurrentUserQuery("missing-user"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["User not found."], result.Errors);
    }
}
