using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;

namespace LibraryManagementApi.Application.Loans.Queries.GetLoans;

public class GetLoansQueryHandler(IApplicationDbContext context) : IRequestHandler<GetLoansQuery, PaginatedList<LoanDto>>
{
    public Task<PaginatedList<LoanDto>> Handle(GetLoansQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var query =
            from l in context.Loans
            join member in context.Members on l.MemberId equals member.Id
            join book in context.Books on l.BookId equals book.Id
            join branch in context.Branches on l.BranchId equals branch.Id
            select new { Loan = l, MemberName = member.FullName, BookTitle = book.Title, BranchName = branch.Name };

        if (request.MemberId is not null)
        {
            query = query.Where(x => x.Loan.MemberId == request.MemberId);
        }

        if (request.BookId is not null)
        {
            query = query.Where(x => x.Loan.BookId == request.BookId);
        }

        if (request.BranchId is not null)
        {
            query = query.Where(x => x.Loan.BranchId == request.BranchId);
        }

        if (request.OnlyOverdue == true)
        {
            query = query.Where(x => x.Loan.Status == LoanStatus.Active && x.Loan.DueDateUtc < now);
        }

        var projected = query
            .OrderByDescending(x => x.Loan.BorrowedAtUtc)
            .Select(x => new LoanDto(
                x.Loan.Id, x.Loan.MemberId, x.MemberName, x.Loan.BookId, x.BookTitle, x.Loan.BranchId, x.BranchName,
                x.Loan.BorrowedAtUtc, x.Loan.DueDateUtc, x.Loan.ReturnedAtUtc, x.Loan.Status,
                x.Loan.Status == LoanStatus.Active && x.Loan.DueDateUtc < now));

        return PaginatedList<LoanDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
