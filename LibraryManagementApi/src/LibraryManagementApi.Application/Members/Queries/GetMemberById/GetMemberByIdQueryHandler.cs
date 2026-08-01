using LibraryManagementApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetMemberByIdQuery, MemberDto?>
{
    public Task<MemberDto?> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        return (
            from m in context.Members.AsNoTracking()
            join branch in context.Branches.AsNoTracking() on m.HomeBranchId equals branch.Id
            where m.Id == request.Id
            select new MemberDto(m.Id, m.MembershipNumber, m.FullName, m.Email, m.Phone, m.Address, m.HomeBranchId, branch.Name, m.Status, m.JoinDateUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
