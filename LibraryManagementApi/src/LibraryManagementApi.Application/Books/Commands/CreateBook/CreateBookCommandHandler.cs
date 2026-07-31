using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Books.Commands.CreateBook;

public class CreateBookCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateBookCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var isbn = IsbnHelper.Normalize(request.Isbn);

        var isbnTaken = await context.Books.AnyAsync(b => b.IsActive && b.Isbn == isbn, cancellationToken);
        if (isbnTaken)
        {
            return Result<BookDto>.Failure(["A book with this ISBN already exists."]);
        }

        var book = Book.Create(request.Title, request.Author, isbn, request.Genre, request.PublishedYear, request.Description);
        context.Books.Add(book);
        await context.SaveChangesAsync(cancellationToken);

        return Result<BookDto>.Success(new BookDto(book.Id, book.Title, book.Author, book.Isbn, book.Genre, book.PublishedYear, book.Description, book.IsActive));
    }
}
