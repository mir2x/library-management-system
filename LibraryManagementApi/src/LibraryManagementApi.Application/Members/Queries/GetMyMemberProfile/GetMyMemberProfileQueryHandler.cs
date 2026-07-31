using LibraryManagementApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Members.Queries.GetMyMemberProfile;

public class GetMyMemberProfileQueryHandler(IApplicationDbContext context) : IRequestHandler<GetMyMemberProfileQuery, MemberDto?>
{
    public Task<MemberDto?> Handle(GetMyMemberProfileQuery request, CancellationToken cancellationToken)
    {
        return (
            from m in context.Members
            join branch in context.Branches on m.HomeBranchId equals branch.Id
            where m.UserId == request.UserId
            select new MemberDto(m.Id, m.MembershipNumber, m.FullName, m.Email, m.Phone, m.Address, m.HomeBranchId, branch.Name, m.Status, m.JoinDateUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
