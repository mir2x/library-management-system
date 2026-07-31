using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Common.Models;

namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IAuthTokenIssuer
{
    Task<AuthResponse> IssueTokensAsync(AuthenticatedUser user, CancellationToken cancellationToken);
}
