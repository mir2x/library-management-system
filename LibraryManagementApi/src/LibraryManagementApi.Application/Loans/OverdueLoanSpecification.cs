using System.Linq.Expressions;
using LibraryManagementApi.Application.Common.Specifications;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Application.Loans;

// "Is this loan overdue" was previously duplicated as an inline .Where(...) clause in both
// GetLoansQueryHandler (the OnlyOverdue filter) and GetOverdueLoansReportQueryHandler (the base
// query) — encapsulated here once so the two can't drift out of sync.
public class OverdueLoanSpecification(DateTime asOfUtc) : Specification<Loan>
{
    public override Expression<Func<Loan, bool>> ToExpression() =>
        loan => loan.Status == LoanStatus.Active && loan.DueDateUtc < asOfUtc;
}
