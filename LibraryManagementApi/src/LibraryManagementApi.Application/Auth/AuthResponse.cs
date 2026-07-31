namespace LibraryManagementApi.Application.Auth;

public record AuthResponse(
    string UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
