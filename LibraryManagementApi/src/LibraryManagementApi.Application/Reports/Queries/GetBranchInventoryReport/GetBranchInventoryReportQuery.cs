using MediatR;

namespace LibraryManagementApi.Application.Reports.Queries.GetBranchInventoryReport;

public record GetBranchInventoryReportQuery(Guid? BranchId) : IRequest<List<BranchInventorySummaryDto>>;
