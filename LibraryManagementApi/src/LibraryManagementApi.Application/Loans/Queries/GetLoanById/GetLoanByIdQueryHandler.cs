using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Loans.Queries.GetLoanById;

public class GetLoanByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetLoanByIdQuery, LoanDto?>
{
    public Task<LoanDto?> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return (
            from l in context.Loans
            join member in context.Members on l.MemberId equals member.Id
            join book in context.Books on l.BookId equals book.Id
            join branch in context.Branches on l.BranchId equals branch.Id
            where l.Id == request.Id
            select new LoanDto(
                l.Id, l.MemberId, member.FullName, l.BookId, book.Title, l.BranchId, branch.Name,
                l.BorrowedAtUtc, l.DueDateUtc, l.ReturnedAtUtc, l.Status,
                l.Status == LoanStatus.Active && l.DueDateUtc < now))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
