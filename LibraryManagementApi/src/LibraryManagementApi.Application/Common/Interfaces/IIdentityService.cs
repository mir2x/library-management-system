using LibraryManagementApi.Application.Common.Models;

namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthenticatedUser>> CreateUserAsync(string email, string password, string fullName, string role, CancellationToken cancellationToken);

    Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}
