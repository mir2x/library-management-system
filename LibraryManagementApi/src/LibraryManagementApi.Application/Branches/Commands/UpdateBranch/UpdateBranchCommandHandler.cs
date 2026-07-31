using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateBranchCommand, Result>
{
    public async Task<Result> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await context.Branches.SingleOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.Id);

        var name = request.Name ?? branch.Name;
        var address = request.Address ?? branch.Address;
        var contactNumber = request.ContactNumber ?? branch.ContactNumber;
        var email = request.Email ?? branch.Email;

        if (!string.Equals(name, branch.Name, StringComparison.OrdinalIgnoreCase))
        {
            var nameTaken = await context.Branches
                .AnyAsync(b => b.Id != branch.Id && b.IsActive && b.Name.ToLower() == name.ToLower(), cancellationToken);

            if (nameTaken)
            {
                return Result.Failure(["A branch with this name already exists."]);
            }
        }

        branch.Update(name, address, contactNumber, email);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
