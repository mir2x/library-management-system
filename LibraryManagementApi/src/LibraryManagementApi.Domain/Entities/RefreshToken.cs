namespace LibraryManagementApi.Domain.Entities;

public class RefreshToken
{
    private RefreshToken()
    {
    }

    private RefreshToken(string token, string userId, DateTime expiresAtUtc)
    {
        Token = token;
        UserId = userId;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public string UserId { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? ReplacedByToken { get; private set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    public static RefreshToken Create(string token, string userId, DateTime expiresAtUtc) =>
        new(token, userId, expiresAtUtc);

    public void Revoke(string? replacedByToken = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
