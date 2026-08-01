using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Loans;

public class OverdueLoanSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithActiveLoanPastDueDate_ReturnsTrue()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var asOfUtc = loan.DueDateUtc.AddDays(1);

        var isOverdue = new OverdueLoanSpecification(asOfUtc).IsSatisfiedBy(loan);

        Assert.True(isOverdue);
    }

    [Fact]
    public void IsSatisfiedBy_WithActiveLoanBeforeDueDate_ReturnsFalse()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var asOfUtc = loan.DueDateUtc.AddDays(-1);

        var isOverdue = new OverdueLoanSpecification(asOfUtc).IsSatisfiedBy(loan);

        Assert.False(isOverdue);
    }

    [Fact]
    public void IsSatisfiedBy_WithReturnedLoanPastDueDate_ReturnsFalse()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        loan.MarkReturned();
        var asOfUtc = loan.DueDateUtc.AddDays(1);

        var isOverdue = new OverdueLoanSpecification(asOfUtc).IsSatisfiedBy(loan);

        Assert.False(isOverdue);
    }
}
