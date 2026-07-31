using LibraryManagementApi.Application.Common.Models;

namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthenticatedUser>> CreateUserAsync(string email, string password, string fullName, string role, CancellationToken cancellationToken);

    Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);

    Task<AuthenticatedUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken);

    /// <returns>The password reset token, or null if no account exists for the given email.</returns>
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
}
