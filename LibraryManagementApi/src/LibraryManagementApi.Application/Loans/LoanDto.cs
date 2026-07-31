using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Application.Loans;

public record LoanDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid BookId,
    string BookTitle,
    Guid BranchId,
    string BranchName,
    DateTime BorrowedAtUtc,
    DateTime DueDateUtc,
    DateTime? ReturnedAtUtc,
    LoanStatus Status,
    bool IsOverdue);
