using LibraryManagementApi.Application.Auth.Commands.Logout;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Logout;

public class LogoutCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _handler = new LogoutCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithActiveToken_RevokesItAndReturnsSuccess()
    {
        var token = RefreshToken.Create("active-token", "user-1", DateTime.UtcNow.AddDays(1));
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new LogoutCommand("active-token"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(token.RevokedAtUtc);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_StillReturnsSuccess()
    {
        var result = await _handler.Handle(new LogoutCommand("token-that-does-not-exist"), CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_IsIdempotentAndReturnsSuccess()
    {
        var token = RefreshToken.Create("already-revoked-token", "user-1", DateTime.UtcNow.AddDays(1));
        token.Revoke();
        var originalRevokedAt = token.RevokedAtUtc;
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new LogoutCommand("already-revoked-token"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(originalRevokedAt, token.RevokedAtUtc);
    }
}
