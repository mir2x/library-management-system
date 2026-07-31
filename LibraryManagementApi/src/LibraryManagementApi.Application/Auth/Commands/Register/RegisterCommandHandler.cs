using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Constants;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.Register;

public class RegisterCommandHandler(IIdentityService identityService, IAuthTokenIssuer authTokenIssuer)
    : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Public self-registration is always as a Member. Librarian/Admin accounts are
        // provisioned separately (see ARCHITECTURE.md / README assumptions).
        var result = await identityService.CreateUserAsync(request.Email, request.Password, request.FullName, Roles.Member, cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return Result<AuthResponse>.Failure(result.Errors);
        }

        var response = await authTokenIssuer.IssueTokensAsync(result.Value, cancellationToken);

        return Result<AuthResponse>.Success(response);
    }
}
