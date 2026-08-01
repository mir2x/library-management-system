using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations.Queries.GetReservationById;

public class GetReservationByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetReservationByIdQuery, ReservationDto?>
{
    public async Task<ReservationDto?> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await (
            from r in context.Reservations.AsNoTracking()
            join member in context.Members.AsNoTracking() on r.MemberId equals member.Id
            join book in context.Books.AsNoTracking() on r.BookId equals book.Id
            join branch in context.Branches.AsNoTracking() on r.BranchId equals branch.Id
            where r.Id == request.Id
            select new { Reservation = r, MemberName = member.FullName, BookTitle = book.Title, BranchName = branch.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        var queuePosition = result.Reservation.Status == ReservationStatus.Pending
            ? await context.Reservations.CountAsync(
                r => r.BookId == result.Reservation.BookId && r.BranchId == result.Reservation.BranchId
                    && r.Status == ReservationStatus.Pending && r.ReservedAtUtc <= result.Reservation.ReservedAtUtc,
                cancellationToken)
            : 0;

        return new ReservationDto(
            result.Reservation.Id, result.Reservation.MemberId, result.MemberName, result.Reservation.BookId, result.BookTitle,
            result.Reservation.BranchId, result.BranchName, result.Reservation.ReservedAtUtc, result.Reservation.ReadyAtUtc,
            result.Reservation.FulfilledAtUtc, result.Reservation.CancelledAtUtc, result.Reservation.Status, queuePosition);
    }
}
