using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations.Commands.CreateMyReservation;

public class CreateMyReservationCommandHandler(IApplicationDbContext context, ReservationCreator creator)
    : IRequestHandler<CreateMyReservationCommand, Result<ReservationDto>>
{
    public async Task<Result<ReservationDto>> Handle(CreateMyReservationCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.UserId);

        return await creator.CreateAsync(member, request.BookId, request.BranchId, cancellationToken);
    }
}
