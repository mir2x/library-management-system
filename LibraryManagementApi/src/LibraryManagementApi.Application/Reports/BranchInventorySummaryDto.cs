namespace LibraryManagementApi.Application.Reports;

public record BranchInventorySummaryDto(
    Guid BranchId,
    string BranchName,
    int TotalTitles,
    int TotalCopies,
    int AvailableCopies,
    double UtilizationPercentage);
