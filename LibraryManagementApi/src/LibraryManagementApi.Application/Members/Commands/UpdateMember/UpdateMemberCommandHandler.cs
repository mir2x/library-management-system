using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateMemberCommand, Result>
{
    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.Id);

        var homeBranchId = request.HomeBranchId ?? member.HomeBranchId;
        if (homeBranchId != member.HomeBranchId)
        {
            var branchExists = await context.Branches.AnyAsync(b => b.Id == homeBranchId, cancellationToken);
            if (!branchExists)
            {
                throw new NotFoundException(nameof(Branch), homeBranchId);
            }
        }

        var email = request.Email ?? member.Email;
        if (!string.Equals(email, member.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await context.Members
                .AnyAsync(m => m.Id != member.Id && m.Status != MembershipStatus.Deactivated && m.Email.ToLower() == email.ToLower(), cancellationToken);

            if (emailTaken)
            {
                return Result.Failure(["A member with this email already exists."]);
            }
        }

        member.Update(
            request.FullName ?? member.FullName,
            email,
            request.Phone ?? member.Phone,
            request.Address ?? member.Address,
            homeBranchId);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
