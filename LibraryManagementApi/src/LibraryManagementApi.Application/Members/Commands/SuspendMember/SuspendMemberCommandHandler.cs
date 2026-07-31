using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Members.Commands.SuspendMember;

public class SuspendMemberCommandHandler(IApplicationDbContext context) : IRequestHandler<SuspendMemberCommand, Result>
{
    public async Task<Result> Handle(SuspendMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.Id);

        member.Suspend();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
