using LibraryManagementApi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Books.Queries.GetBookById;

public class GetBookByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetBookByIdQuery, BookDetailDto?>
{
    public async Task<BookDetailDto?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await context.Books.AsNoTracking().SingleOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (book is null)
        {
            return null;
        }

        var inventory = await (
            from i in context.BookInventories.AsNoTracking()
            join branch in context.Branches.AsNoTracking() on i.BranchId equals branch.Id
            where i.BookId == book.Id
            orderby branch.Name
            select new BookInventoryDto(branch.Id, branch.Name, i.TotalCopies, i.AvailableCopies))
            .ToListAsync(cancellationToken);

        return new BookDetailDto(book.Id, book.Title, book.Author, book.Isbn, book.Genre, book.PublishedYear, book.Description, book.IsActive, inventory);
    }
}
