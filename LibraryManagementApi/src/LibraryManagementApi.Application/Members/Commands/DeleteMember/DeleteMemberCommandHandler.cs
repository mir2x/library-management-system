using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteMemberCommand, Result>
{
    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.Id);

        member.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
