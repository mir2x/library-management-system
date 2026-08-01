using LibraryManagementApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reports.Queries.GetMostBorrowedBooksReport;

public class GetMostBorrowedBooksReportQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMostBorrowedBooksReportQuery, List<MostBorrowedBookDto>>
{
    public async Task<List<MostBorrowedBookDto>> Handle(GetMostBorrowedBooksReportQuery request, CancellationToken cancellationToken)
    {
        var loans = context.Loans.AsNoTracking();

        if (request.BranchId is not null)
        {
            loans = loans.Where(l => l.BranchId == request.BranchId);
        }

        if (request.FromUtc is not null)
        {
            loans = loans.Where(l => l.BorrowedAtUtc >= request.FromUtc);
        }

        if (request.ToUtc is not null)
        {
            loans = loans.Where(l => l.BorrowedAtUtc <= request.ToUtc);
        }

        var counted =
            from l in loans
            group l by l.BookId into g
            select new { BookId = g.Key, BorrowCount = g.Count() };

        var query =
            from c in counted
            join book in context.Books.AsNoTracking() on c.BookId equals book.Id
            orderby c.BorrowCount descending
            select new MostBorrowedBookDto(book.Id, book.Title, book.Author, c.BorrowCount);

        return await query.Take(request.Top).ToListAsync(cancellationToken);
    }
}
