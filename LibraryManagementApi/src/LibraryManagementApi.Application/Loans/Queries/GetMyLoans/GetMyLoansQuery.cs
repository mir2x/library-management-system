using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Loans.Queries.GetMyLoans;

public record GetMyLoansQuery(string UserId, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<LoanDto>>;
