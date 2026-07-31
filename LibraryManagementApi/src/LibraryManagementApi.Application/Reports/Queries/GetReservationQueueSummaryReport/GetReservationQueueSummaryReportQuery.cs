using MediatR;

namespace LibraryManagementApi.Application.Reports.Queries.GetReservationQueueSummaryReport;

public record GetReservationQueueSummaryReportQuery(Guid? BranchId) : IRequest<List<ReservationQueueSummaryDto>>;
