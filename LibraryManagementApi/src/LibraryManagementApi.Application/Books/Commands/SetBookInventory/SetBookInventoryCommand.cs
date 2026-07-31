using MediatR;

namespace LibraryManagementApi.Application.Books.Commands.SetBookInventory;

public record SetBookInventoryCommand(Guid BookId, Guid BranchId, int TotalCopies) : IRequest<BookInventoryDto>;
