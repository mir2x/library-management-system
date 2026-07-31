using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.Common.Services;

public class AuthTokenIssuer(IJwtTokenGenerator jwtTokenGenerator, IApplicationDbContext context) : IAuthTokenIssuer
{
    public async Task<AuthResponse> IssueTokensAsync(AuthenticatedUser user, CancellationToken cancellationToken)
    {
        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(refreshToken.Value, user.Id, refreshToken.ExpiresAtUtc);
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Roles,
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            refreshToken.Value,
            refreshToken.ExpiresAtUtc);
    }
}
