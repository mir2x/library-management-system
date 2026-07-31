using MediatR;

namespace LibraryManagementApi.Application.Reports.Queries.GetMostBorrowedBooksReport;

public record GetMostBorrowedBooksReportQuery(
    Guid? BranchId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Top = 10) : IRequest<List<MostBorrowedBookDto>>;
