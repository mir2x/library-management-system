namespace LibraryManagementApi.Application.Reports;

public record ReservationQueueSummaryDto(
    Guid BookId,
    string BookTitle,
    Guid BranchId,
    string BranchName,
    int PendingCount,
    bool HasReadyCopy,
    DateTime? OldestPendingSinceUtc);
