using LibraryManagementApi.Domain.Common;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.Entities;

public class Loan : BaseAuditableEntity
{
    public const int LoanPeriodDays = 14;

    public const int MaxActiveLoansPerMember = 5;

    private Loan()
    {
    }

    private Loan(Guid memberId, Guid bookId, Guid branchId)
    {
        MemberId = memberId;
        BookId = bookId;
        BranchId = branchId;
        BorrowedAtUtc = DateTime.UtcNow;
        DueDateUtc = BorrowedAtUtc.AddDays(LoanPeriodDays);
        Status = LoanStatus.Active;
    }

    public Guid MemberId { get; private set; }

    public Guid BookId { get; private set; }

    public Guid BranchId { get; private set; }

    public DateTime BorrowedAtUtc { get; private set; }

    public DateTime DueDateUtc { get; private set; }

    public DateTime? ReturnedAtUtc { get; private set; }

    public LoanStatus Status { get; private set; }

    // Overdue is a time-derived fact, not a state anyone sets, so it's never persisted —
    // computing it here (rather than storing a third status) means it can't go stale.
    public bool IsOverdue => Status == LoanStatus.Active && DateTime.UtcNow > DueDateUtc;

    public static Loan Create(Guid memberId, Guid bookId, Guid branchId) => new(memberId, bookId, branchId);

    public void MarkReturned()
    {
        if (Status == LoanStatus.Returned)
        {
            throw new DomainException("This loan has already been returned.");
        }

        Status = LoanStatus.Returned;
        ReturnedAtUtc = DateTime.UtcNow;
    }
}
