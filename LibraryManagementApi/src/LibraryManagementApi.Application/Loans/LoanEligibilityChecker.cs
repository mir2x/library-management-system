using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Loans;

// Shared between BorrowBookCommandHandler and FulfillReservationCommandHandler — a reservation
// pickup is still "receiving a new loan" and must satisfy the same eligibility rules as a
// direct borrow.
public class LoanEligibilityChecker(IApplicationDbContext context)
{
    /// <returns>Null if eligible, otherwise a user-facing reason the member cannot borrow.</returns>
    public async Task<string?> CheckAsync(Member member, Guid bookId, CancellationToken cancellationToken)
    {
        if (member.Status != MembershipStatus.Active)
        {
            return "Member is not active and cannot borrow books.";
        }

        var activeLoanCount = await context.Loans
            .CountAsync(l => l.MemberId == member.Id && l.Status == LoanStatus.Active, cancellationToken);

        if (activeLoanCount >= Loan.MaxActiveLoansPerMember)
        {
            return $"Member has reached the maximum of {Loan.MaxActiveLoansPerMember} active loans.";
        }

        var alreadyBorrowed = await context.Loans
            .AnyAsync(l => l.MemberId == member.Id && l.BookId == bookId && l.Status == LoanStatus.Active, cancellationToken);

        return alreadyBorrowed ? "Member already has an active loan for this book." : null;
    }
}
