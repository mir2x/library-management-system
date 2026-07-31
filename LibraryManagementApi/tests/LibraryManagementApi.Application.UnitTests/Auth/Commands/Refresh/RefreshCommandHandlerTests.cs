using LibraryManagementApi.Application.Auth.Commands.Refresh;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Constants;
using LibraryManagementApi.Domain.Entities;
using NSubstitute;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Refresh;

public class RefreshCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly RefreshCommandHandler _handler;

    public RefreshCommandHandlerTests()
    {
        _handler = new RefreshCommandHandler(_context, _identityService, _jwtTokenGenerator);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsFailure()
    {
        var command = new RefreshCommand("token-that-does-not-exist");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Invalid refresh token."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithActiveToken_RotatesTokenAndReturnsNewTokens()
    {
        var user = new AuthenticatedUser("user-1", "jane.doe@example.com", "Jane Doe", [Roles.Member]);
        var existingToken = RefreshToken.Create("current-refresh-token", user.Id, DateTime.UtcNow.AddDays(1));
        _context.RefreshTokens.Add(existingToken);
        await _context.SaveChangesAsync(CancellationToken.None);

        var newAccessToken = new AuthToken("new-access-token", DateTime.UtcNow.AddMinutes(15));
        var newRefreshToken = new AuthToken("new-refresh-token", DateTime.UtcNow.AddDays(7));

        _identityService.GetUserByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenGenerator.GenerateAccessToken(user).Returns(newAccessToken);
        _jwtTokenGenerator.GenerateRefreshToken().Returns(newRefreshToken);

        var command = new RefreshCommand("current-refresh-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(newAccessToken.Value, result.Value!.AccessToken);
        Assert.Equal(newRefreshToken.Value, result.Value!.RefreshToken);

        Assert.NotNull(existingToken.RevokedAtUtc);
        Assert.Equal(newRefreshToken.Value, existingToken.ReplacedByToken);

        var persistedNewToken = _context.RefreshTokens.SingleOrDefault(rt => rt.Token == newRefreshToken.Value);
        Assert.NotNull(persistedNewToken);
        Assert.True(persistedNewToken!.IsActive);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsFailure()
    {
        var expiredToken = RefreshToken.Create("expired-token", "user-1", DateTime.UtcNow.AddDays(-1));
        _context.RefreshTokens.Add(expiredToken);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new RefreshCommand("expired-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Refresh token has expired."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_RevokesAllActiveTokensForUserAndReturnsFailure()
    {
        const string userId = "user-1";

        var reusedToken = RefreshToken.Create("stolen-token", userId, DateTime.UtcNow.AddDays(1));
        reusedToken.Revoke("some-other-token");

        var otherActiveToken = RefreshToken.Create("other-active-token", userId, DateTime.UtcNow.AddDays(1));

        _context.RefreshTokens.AddRange(reusedToken, otherActiveToken);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new RefreshCommand("stolen-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Refresh token has been revoked. Please log in again."], result.Errors);

        var refreshedOtherToken = _context.RefreshTokens.Single(rt => rt.Token == "other-active-token");
        Assert.NotNull(refreshedOtherToken.RevokedAtUtc);

        _jwtTokenGenerator.DidNotReceive().GenerateAccessToken(Arg.Any<AuthenticatedUser>());
    }
}
