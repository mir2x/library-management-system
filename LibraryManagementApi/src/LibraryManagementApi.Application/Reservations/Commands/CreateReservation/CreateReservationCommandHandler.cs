using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler(IApplicationDbContext context, ReservationCreator creator)
    : IRequestHandler<CreateReservationCommand, Result<ReservationDto>>
{
    public async Task<Result<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        return await creator.CreateAsync(member, request.BookId, request.BranchId, cancellationToken);
    }
}
