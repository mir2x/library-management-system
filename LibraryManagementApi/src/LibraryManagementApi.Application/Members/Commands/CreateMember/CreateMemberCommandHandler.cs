using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateMemberCommand, Result<MemberDto>>
{
    public async Task<Result<MemberDto>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var branch = await context.Branches.SingleOrDefaultAsync(b => b.Id == request.HomeBranchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.HomeBranchId);

        var emailTaken = await context.Members
            .AnyAsync(m => m.Status != MembershipStatus.Deactivated && m.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (emailTaken)
        {
            return Result<MemberDto>.Failure(["A member with this email already exists."]);
        }

        var member = Member.Create(
            MembershipNumberGenerator.Generate(), request.FullName, request.Email, request.Phone, request.Address, request.HomeBranchId, userId: null);

        context.Members.Add(member);
        await context.SaveChangesAsync(cancellationToken);

        return Result<MemberDto>.Success(new MemberDto(
            member.Id, member.MembershipNumber, member.FullName, member.Email, member.Phone, member.Address,
            member.HomeBranchId, branch.Name, member.Status, member.JoinDateUtc));
    }
}
