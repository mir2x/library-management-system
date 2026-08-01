using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetMyReservationsQuery, PaginatedList<ReservationDto>>
{
    public async Task<PaginatedList<ReservationDto>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var member = await context.Members.AsNoTracking().SingleOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);
        if (member is null)
        {
            return new PaginatedList<ReservationDto>([], 0, request.PageNumber, request.PageSize);
        }

        var query =
            from r in context.Reservations.AsNoTracking()
            join book in context.Books.AsNoTracking() on r.BookId equals book.Id
            join branch in context.Branches.AsNoTracking() on r.BranchId equals branch.Id
            where r.MemberId == member.Id
            orderby r.ReservedAtUtc
            select new { Reservation = r, BookTitle = book.Title, BranchName = branch.Name };

        var totalCount = await query.CountAsync(cancellationToken);
        var pageItems = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<ReservationDto>(pageItems.Count);
        foreach (var item in pageItems)
        {
            var queuePosition = item.Reservation.Status == ReservationStatus.Pending
                ? await context.Reservations.CountAsync(
                    r => r.BookId == item.Reservation.BookId && r.BranchId == item.Reservation.BranchId
                        && r.Status == ReservationStatus.Pending && r.ReservedAtUtc <= item.Reservation.ReservedAtUtc,
                    cancellationToken)
                : 0;

            dtos.Add(new ReservationDto(
                item.Reservation.Id, item.Reservation.MemberId, member.FullName, item.Reservation.BookId, item.BookTitle,
                item.Reservation.BranchId, item.BranchName, item.Reservation.ReservedAtUtc, item.Reservation.ReadyAtUtc,
                item.Reservation.FulfilledAtUtc, item.Reservation.CancelledAtUtc, item.Reservation.Status, queuePosition));
        }

        return new PaginatedList<ReservationDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
