namespace LibraryManagementApi.Application.Reports;

public record OverdueLoanDto(
    Guid LoanId,
    Guid MemberId,
    string MemberName,
    Guid BookId,
    string BookTitle,
    Guid BranchId,
    string BranchName,
    DateTime BorrowedAtUtc,
    DateTime DueDateUtc,
    int DaysOverdue);
