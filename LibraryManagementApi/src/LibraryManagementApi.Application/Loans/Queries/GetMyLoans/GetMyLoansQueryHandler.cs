using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Loans.Queries.GetMyLoans;

public class GetMyLoansQueryHandler(IApplicationDbContext context) : IRequestHandler<GetMyLoansQuery, PaginatedList<LoanDto>>
{
    public async Task<PaginatedList<LoanDto>> Handle(GetMyLoansQuery request, CancellationToken cancellationToken)
    {
        var member = await context.Members.AsNoTracking().SingleOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);
        if (member is null)
        {
            return new PaginatedList<LoanDto>([], 0, request.PageNumber, request.PageSize);
        }

        var now = DateTime.UtcNow;

        var projected =
            from l in context.Loans.AsNoTracking()
            join book in context.Books.AsNoTracking() on l.BookId equals book.Id
            join branch in context.Branches.AsNoTracking() on l.BranchId equals branch.Id
            where l.MemberId == member.Id
            orderby l.BorrowedAtUtc descending
            select new LoanDto(
                l.Id, l.MemberId, member.FullName, l.BookId, book.Title, l.BranchId, branch.Name,
                l.BorrowedAtUtc, l.DueDateUtc, l.ReturnedAtUtc, l.Status,
                l.Status == LoanStatus.Active && l.DueDateUtc < now);

        return await PaginatedList<LoanDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
