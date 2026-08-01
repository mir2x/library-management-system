using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Branches.Queries.GetBranches;

public class GetBranchesQueryHandler(IApplicationDbContext context) : IRequestHandler<GetBranchesQuery, PaginatedList<BranchDto>>
{
    public Task<PaginatedList<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Branches.AsNoTracking().Where(b => b.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(search) || b.Address.ToLower().Contains(search));
        }

        var projected = query
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.ContactNumber, b.Email, b.IsActive));

        return PaginatedList<BranchDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
