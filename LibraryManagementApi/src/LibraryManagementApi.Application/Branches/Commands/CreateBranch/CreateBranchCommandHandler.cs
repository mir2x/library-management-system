using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateBranchCommand, Result<BranchDto>>
{
    public async Task<Result<BranchDto>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var nameTaken = await context.Branches
            .AnyAsync(b => b.IsActive && b.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (nameTaken)
        {
            return Result<BranchDto>.Failure(["A branch with this name already exists."]);
        }

        var branch = Branch.Create(request.Name, request.Address, request.ContactNumber, request.Email);
        context.Branches.Add(branch);
        await context.SaveChangesAsync(cancellationToken);

        return Result<BranchDto>.Success(new BranchDto(branch.Id, branch.Name, branch.Address, branch.ContactNumber, branch.Email, branch.IsActive));
    }
}
