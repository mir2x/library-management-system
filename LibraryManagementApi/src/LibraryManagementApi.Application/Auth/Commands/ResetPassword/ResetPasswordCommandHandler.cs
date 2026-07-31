using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IIdentityService identityService) : IRequestHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken) =>
        identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
}
