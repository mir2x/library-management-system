using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Books.Queries.GetBooks;

public record GetBooksQuery(string? Search, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<BookDto>>;
