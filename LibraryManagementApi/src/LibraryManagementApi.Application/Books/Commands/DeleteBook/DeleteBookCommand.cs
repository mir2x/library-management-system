using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Books.Commands.DeleteBook;

public record DeleteBookCommand(Guid Id) : IRequest<Result>;
