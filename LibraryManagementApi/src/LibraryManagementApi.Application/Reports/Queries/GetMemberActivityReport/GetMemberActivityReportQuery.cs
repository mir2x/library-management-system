using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Reports.Queries.GetMemberActivityReport;

public record GetMemberActivityReportQuery(
    Guid? BranchId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<MemberActivityDto>>;
