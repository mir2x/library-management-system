using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Auth.Commands.Refresh;

public class RefreshCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<RefreshCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingToken is null)
        {
            return Result<AuthResponse>.Failure(["Invalid refresh token."]);
        }

        if (existingToken.RevokedAtUtc is not null)
        {
            // This token was already rotated away (or logged out) but is being presented again.
            // Treat that as a possible theft/replay and revoke every other active token for the
            // user, forcing a fresh login instead of trusting the compromised token family.
            await RevokeAllActiveTokensForUserAsync(existingToken.UserId, cancellationToken);

            return Result<AuthResponse>.Failure(["Refresh token has been revoked. Please log in again."]);
        }

        if (!existingToken.IsActive)
        {
            return Result<AuthResponse>.Failure(["Refresh token has expired."]);
        }

        var user = await identityService.GetUserByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result<AuthResponse>.Failure(["Invalid refresh token."]);
        }

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = jwtTokenGenerator.GenerateRefreshToken();

        existingToken.Revoke(newRefreshToken.Value);
        context.RefreshTokens.Add(RefreshToken.Create(newRefreshToken.Value, user.Id, newRefreshToken.ExpiresAtUtc));

        await context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Roles,
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            newRefreshToken.Value,
            newRefreshToken.ExpiresAtUtc));
    }

    private async Task RevokeAllActiveTokensForUserAsync(string userId, CancellationToken cancellationToken)
    {
        var activeTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        if (activeTokens.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
