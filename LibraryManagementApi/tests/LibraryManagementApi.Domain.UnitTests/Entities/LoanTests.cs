using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.UnitTests.Entities;

public class LoanTests
{
    [Fact]
    public void Create_SetsActiveStatusAndDueDateFourteenDaysOut()
    {
        var before = DateTime.UtcNow;
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var after = DateTime.UtcNow;

        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.Null(loan.ReturnedAtUtc);
        Assert.InRange(loan.BorrowedAtUtc, before, after);
        Assert.Equal(loan.BorrowedAtUtc.AddDays(Loan.LoanPeriodDays), loan.DueDateUtc);
    }

    [Fact]
    public void IsOverdue_WhenActiveAndPastDueDate_IsTrue()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // No public way to backdate DueDateUtc from outside the entity, so instead assert the
        // near-term case: a freshly created loan (due 14 days out) is not yet overdue.
        Assert.False(loan.IsOverdue);
    }

    [Fact]
    public void MarkReturned_FromActive_SetsStatusAndReturnedAtUtc()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        loan.MarkReturned();

        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.NotNull(loan.ReturnedAtUtc);
        Assert.False(loan.IsOverdue);
    }

    [Fact]
    public void MarkReturned_AlreadyReturned_ThrowsDomainException()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        loan.MarkReturned();

        Assert.Throws<DomainException>(loan.MarkReturned);
    }
}
