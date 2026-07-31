using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Loans.Queries.GetLoans;

public record GetLoansQuery(
    Guid? MemberId,
    Guid? BookId,
    Guid? BranchId,
    bool? OnlyOverdue,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<LoanDto>>;
