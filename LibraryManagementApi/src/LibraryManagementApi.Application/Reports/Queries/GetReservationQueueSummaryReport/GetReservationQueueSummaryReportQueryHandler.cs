using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reports.Queries.GetReservationQueueSummaryReport;

public class GetReservationQueueSummaryReportQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReservationQueueSummaryReportQuery, List<ReservationQueueSummaryDto>>
{
    public async Task<List<ReservationQueueSummaryDto>> Handle(GetReservationQueueSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var reservations = context.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready);

        if (request.BranchId is not null)
        {
            reservations = reservations.Where(r => r.BranchId == request.BranchId);
        }

        var query =
            from r in reservations
            join book in context.Books.AsNoTracking() on r.BookId equals book.Id
            join branch in context.Branches.AsNoTracking() on r.BranchId equals branch.Id
            select new { r.BookId, BookTitle = book.Title, r.BranchId, BranchName = branch.Name, r.Status, r.ReservedAtUtc };

        var rows = await query.ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => new { x.BookId, x.BookTitle, x.BranchId, x.BranchName })
            .Select(g => new ReservationQueueSummaryDto(
                g.Key.BookId,
                g.Key.BookTitle,
                g.Key.BranchId,
                g.Key.BranchName,
                g.Count(x => x.Status == ReservationStatus.Pending),
                g.Any(x => x.Status == ReservationStatus.Ready),
                g.Where(x => x.Status == ReservationStatus.Pending)
                    .Select(x => (DateTime?)x.ReservedAtUtc)
                    .Min()))
            .OrderByDescending(x => x.PendingCount)
            .ToList();
    }
}
