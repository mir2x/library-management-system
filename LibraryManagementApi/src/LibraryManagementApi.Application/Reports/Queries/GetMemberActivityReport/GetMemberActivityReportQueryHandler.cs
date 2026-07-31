using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;

namespace LibraryManagementApi.Application.Reports.Queries.GetMemberActivityReport;

public class GetMemberActivityReportQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMemberActivityReportQuery, PaginatedList<MemberActivityDto>>
{
    public Task<PaginatedList<MemberActivityDto>> Handle(GetMemberActivityReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var members = context.Members.AsQueryable();

        if (request.BranchId is not null)
        {
            members = members.Where(m => m.HomeBranchId == request.BranchId);
        }

        var projected =
            from m in members
            orderby m.FullName
            select new MemberActivityDto(
                m.Id,
                m.MembershipNumber,
                m.FullName,
                context.Loans.Count(l => l.MemberId == m.Id && l.Status == LoanStatus.Active),
                context.Loans.Count(l => l.MemberId == m.Id),
                context.Loans.Count(l => l.MemberId == m.Id && l.Status == LoanStatus.Active && l.DueDateUtc < now),
                context.Reservations.Count(r => r.MemberId == m.Id
                    && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready)));

        return PaginatedList<MemberActivityDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
