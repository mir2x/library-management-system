using LibraryManagementApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reports.Queries.GetBranchInventoryReport;

public class GetBranchInventoryReportQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBranchInventoryReportQuery, List<BranchInventorySummaryDto>>
{
    public async Task<List<BranchInventorySummaryDto>> Handle(GetBranchInventoryReportQuery request, CancellationToken cancellationToken)
    {
        var branches = context.Branches.AsNoTracking().Where(b => b.IsActive);

        if (request.BranchId is not null)
        {
            branches = branches.Where(b => b.Id == request.BranchId);
        }

        var query =
            from b in branches
            let totalTitles = context.BookInventories.Count(i => i.BranchId == b.Id)
            let totalCopies = context.BookInventories.Where(i => i.BranchId == b.Id).Sum(i => (int?)i.TotalCopies) ?? 0
            let availableCopies = context.BookInventories.Where(i => i.BranchId == b.Id).Sum(i => (int?)i.AvailableCopies) ?? 0
            orderby b.Name
            select new { b.Id, b.Name, totalTitles, totalCopies, availableCopies };

        var branchStats = await query.ToListAsync(cancellationToken);

        return branchStats
            .Select(x => new BranchInventorySummaryDto(
                x.Id,
                x.Name,
                x.totalTitles,
                x.totalCopies,
                x.availableCopies,
                x.totalCopies == 0 ? 0 : Math.Round((x.totalCopies - x.availableCopies) * 100.0 / x.totalCopies, 2)))
            .ToList();
    }
}
