using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations.Commands.CancelReservation;

public class CancelReservationCommandHandler(IApplicationDbContext context, ReservationAllocator reservationAllocator)
    : IRequestHandler<CancelReservationCommand, Result>
{
    public async Task<Result> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations.SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        if (!request.CallerIsStaff)
        {
            var callerMember = await context.Members.SingleOrDefaultAsync(m => m.UserId == request.CallerUserId, cancellationToken);
            if (callerMember is null || callerMember.Id != reservation.MemberId)
            {
                throw new ForbiddenAccessException("You can only cancel your own reservations.");
            }
        }

        var wasReady = reservation.Status == ReservationStatus.Ready;
        reservation.Cancel();

        if (wasReady)
        {
            // The copy was being held for this reservation — releasing it must go through the
            // same "next in line, or general availability" decision as a normal return.
            await reservationAllocator.ReleaseCopyAsync(reservation.BookId, reservation.BranchId, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
