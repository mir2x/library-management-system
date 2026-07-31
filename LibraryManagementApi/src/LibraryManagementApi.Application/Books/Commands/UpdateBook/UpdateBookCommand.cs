using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Books.Commands.UpdateBook;

public record UpdateBookCommand(Guid Id, string? Title, string? Author, string? Genre, int? PublishedYear, string? Description)
    : IRequest<Result>;
