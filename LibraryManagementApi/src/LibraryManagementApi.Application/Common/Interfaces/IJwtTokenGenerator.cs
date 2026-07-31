using LibraryManagementApi.Application.Common.Models;

namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    AuthToken GenerateAccessToken(AuthenticatedUser user);

    AuthToken GenerateRefreshToken();
}

public record AuthToken(string Value, DateTime ExpiresAtUtc);
