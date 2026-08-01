using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations.Queries.GetReservations;

public class GetReservationsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetReservationsQuery, PaginatedList<ReservationDto>>
{
    public async Task<PaginatedList<ReservationDto>> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
    {
        var query =
            from r in context.Reservations.AsNoTracking()
            join member in context.Members.AsNoTracking() on r.MemberId equals member.Id
            join book in context.Books.AsNoTracking() on r.BookId equals book.Id
            join branch in context.Branches.AsNoTracking() on r.BranchId equals branch.Id
            select new { Reservation = r, MemberName = member.FullName, BookTitle = book.Title, BranchName = branch.Name };

        if (request.MemberId is not null)
        {
            query = query.Where(x => x.Reservation.MemberId == request.MemberId);
        }

        if (request.BookId is not null)
        {
            query = query.Where(x => x.Reservation.BookId == request.BookId);
        }

        if (request.BranchId is not null)
        {
            query = query.Where(x => x.Reservation.BranchId == request.BranchId);
        }

        if (request.Status is not null)
        {
            query = query.Where(x => x.Reservation.Status == request.Status);
        }

        var ordered = query.OrderBy(x => x.Reservation.ReservedAtUtc);

        var totalCount = await ordered.CountAsync(cancellationToken);
        var pageItems = await ordered
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
                item.Reservation.Id, item.Reservation.MemberId, item.MemberName, item.Reservation.BookId, item.BookTitle,
                item.Reservation.BranchId, item.BranchName, item.Reservation.ReservedAtUtc, item.Reservation.ReadyAtUtc,
                item.Reservation.FulfilledAtUtc, item.Reservation.CancelledAtUtc, item.Reservation.Status, queuePosition));
        }

        return new PaginatedList<ReservationDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
