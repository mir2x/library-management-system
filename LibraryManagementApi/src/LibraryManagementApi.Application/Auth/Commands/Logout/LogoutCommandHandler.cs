using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(IApplicationDbContext context) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        // Idempotent by design: whether the token was unknown, already revoked, or expired,
        // the caller's intent ("end this session") is satisfied either way, and we don't
        // leak whether a given refresh token value ever existed.
        if (existingToken is not null && existingToken.RevokedAtUtc is null)
        {
            existingToken.Revoke();
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
