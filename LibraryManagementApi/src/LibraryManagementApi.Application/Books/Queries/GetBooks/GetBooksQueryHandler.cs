using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Books.Queries.GetBooks;

public class GetBooksQueryHandler(IApplicationDbContext context) : IRequestHandler<GetBooksQuery, PaginatedList<BookDto>>
{
    public Task<PaginatedList<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        var query = context.Books.AsNoTracking().Where(b => b.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(search) ||
                b.Author.ToLower().Contains(search) ||
                b.Isbn.ToLower().Contains(search) ||
                b.Genre.ToLower().Contains(search));
        }

        var projected = query
            .OrderBy(b => b.Title)
            .Select(b => new BookDto(b.Id, b.Title, b.Author, b.Isbn, b.Genre, b.PublishedYear, b.Description, b.IsActive));

        return PaginatedList<BookDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
