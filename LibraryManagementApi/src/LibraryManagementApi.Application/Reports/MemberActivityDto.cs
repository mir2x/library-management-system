namespace LibraryManagementApi.Application.Reports;

public record MemberActivityDto(
    Guid MemberId,
    string MembershipNumber,
    string MemberName,
    int ActiveLoanCount,
    int TotalLoanCount,
    int OverdueLoanCount,
    int ActiveReservationCount);
