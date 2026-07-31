using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Reports.Queries.GetOverdueLoansReport;

public record GetOverdueLoansReportQuery(
    Guid? BranchId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<OverdueLoanDto>>;
