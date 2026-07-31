using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Members.Commands.ReactivateMember;

public class ReactivateMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<ReactivateMemberCommand, Result>
{
    public async Task<Result> Handle(ReactivateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.Id);

        member.Reactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
