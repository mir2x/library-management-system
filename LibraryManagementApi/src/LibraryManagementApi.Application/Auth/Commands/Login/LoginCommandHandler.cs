using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.Login;

public class LoginCommandHandler(IIdentityService identityService, IAuthTokenIssuer authTokenIssuer)
    : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);

        if (user is null)
        {
            return Result<AuthResponse>.Failure(["Invalid email or password."]);
        }

        var response = await authTokenIssuer.IssueTokensAsync(user, cancellationToken);

        return Result<AuthResponse>.Success(response);
    }
}
