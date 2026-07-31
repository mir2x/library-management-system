using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;

namespace LibraryManagementApi.Application.Members.Queries.GetMembers;

public class GetMembersQueryHandler(IApplicationDbContext context) : IRequestHandler<GetMembersQuery, PaginatedList<MemberDto>>
{
    public Task<PaginatedList<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var query =
            from m in context.Members
            join branch in context.Branches on m.HomeBranchId equals branch.Id
            where m.Status != MembershipStatus.Deactivated
            select new { Member = m, BranchName = branch.Name };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Member.FullName.ToLower().Contains(search) ||
                x.Member.Email.ToLower().Contains(search) ||
                x.Member.MembershipNumber.ToLower().Contains(search));
        }

        var projected = query
            .OrderBy(x => x.Member.FullName)
            .Select(x => new MemberDto(
                x.Member.Id, x.Member.MembershipNumber, x.Member.FullName, x.Member.Email, x.Member.Phone, x.Member.Address,
                x.Member.HomeBranchId, x.BranchName, x.Member.Status, x.Member.JoinDateUtc));

        return PaginatedList<MemberDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
