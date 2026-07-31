using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    IAppUrlProvider appUrlProvider)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var token = await identityService.GeneratePasswordResetTokenAsync(request.Email, cancellationToken);

        // Always respond the same way whether or not the account exists, so this endpoint
        // can't be used to enumerate registered emails. The only observable difference is
        // whether an email actually goes out.
        if (token is not null)
        {
            var resetUrl = appUrlProvider.BuildPasswordResetUrl(request.Email, token);
            var body = $"""
                <p>We received a request to reset your Library Management System password.</p>
                <p><a href="{resetUrl}">Click here to choose a new password</a>. This link can only be used once.</p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                """;

            await emailSender.SendAsync(request.Email, "Reset your password", body, cancellationToken);
        }

        return Result.Success();
    }
}
