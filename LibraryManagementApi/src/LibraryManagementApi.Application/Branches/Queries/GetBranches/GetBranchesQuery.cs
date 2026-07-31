using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Branches.Queries.GetBranches;

public record GetBranchesQuery(string? Search, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<BranchDto>>;
