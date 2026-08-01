using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reports.Queries.GetOverdueLoansReport;

public class GetOverdueLoansReportQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOverdueLoansReportQuery, PaginatedList<OverdueLoanDto>>
{
    public Task<PaginatedList<OverdueLoanDto>> Handle(GetOverdueLoansReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var query =
            from l in context.Loans.AsNoTracking()
            join member in context.Members.AsNoTracking() on l.MemberId equals member.Id
            join book in context.Books.AsNoTracking() on l.BookId equals book.Id
            join branch in context.Branches.AsNoTracking() on l.BranchId equals branch.Id
            where l.Status == LoanStatus.Active && l.DueDateUtc < now
            select new { Loan = l, MemberName = member.FullName, BookTitle = book.Title, BranchName = branch.Name };

        if (request.BranchId is not null)
        {
            query = query.Where(x => x.Loan.BranchId == request.BranchId);
        }

        var projected = query
            .OrderBy(x => x.Loan.DueDateUtc)
            .Select(x => new OverdueLoanDto(
                x.Loan.Id, x.Loan.MemberId, x.MemberName, x.Loan.BookId, x.BookTitle, x.Loan.BranchId, x.BranchName,
                x.Loan.BorrowedAtUtc, x.Loan.DueDateUtc, (now.Date - x.Loan.DueDateUtc.Date).Days));

        return PaginatedList<OverdueLoanDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
