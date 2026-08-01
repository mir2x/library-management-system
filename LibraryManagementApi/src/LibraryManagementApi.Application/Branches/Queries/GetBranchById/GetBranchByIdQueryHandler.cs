using LibraryManagementApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetBranchByIdQuery, BranchDto?>
{
    public Task<BranchDto?> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        // Intentionally not filtered by IsActive: an Admin/Librarian may need to look up a
        // deactivated branch (e.g. before editing or reactivating it).
        return context.Branches
            .AsNoTracking()
            .Where(b => b.Id == request.Id)
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.ContactNumber, b.Email, b.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
