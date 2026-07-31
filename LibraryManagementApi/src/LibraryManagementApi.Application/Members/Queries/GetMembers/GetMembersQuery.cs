using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Members.Queries.GetMembers;

public record GetMembersQuery(string? Search, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<MemberDto>>;
