using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Books.Commands.CreateBook;

public record CreateBookCommand(string Title, string Author, string Isbn, string Genre, int PublishedYear, string? Description)
    : IRequest<Result<BookDto>>;
