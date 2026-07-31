using MediatR;

namespace LibraryManagementApi.Application.Books.Queries.GetBookById;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDetailDto?>;
