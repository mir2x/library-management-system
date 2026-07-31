using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Members;
using LibraryManagementApi.Domain.Constants;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Auth.Commands.Register;

public class RegisterCommandHandler(IIdentityService identityService, IAuthTokenIssuer authTokenIssuer, IApplicationDbContext context)
    : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var branchExists = await context.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken);
        if (!branchExists)
        {
            throw new NotFoundException(nameof(Branch), request.BranchId);
        }

        // Public self-registration is always as a Member. Librarian/Admin accounts are
        // provisioned separately (see ARCHITECTURE.md / README assumptions).
        var result = await identityService.CreateUserAsync(request.Email, request.Password, request.FullName, Roles.Member, cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return Result<AuthResponse>.Failure(result.Errors);
        }

        // Registering is what makes someone a library member, not just a login — so create
        // the linked Member profile in the same flow rather than requiring a separate staff step.
        var member = Member.Create(
            MembershipNumberGenerator.Generate(), result.Value.FullName, result.Value.Email, phone: null, address: null, request.BranchId, result.Value.Id);
        context.Members.Add(member);
        await context.SaveChangesAsync(cancellationToken);

        var response = await authTokenIssuer.IssueTokensAsync(result.Value, cancellationToken);

        return Result<AuthResponse>.Success(response);
    }
}
