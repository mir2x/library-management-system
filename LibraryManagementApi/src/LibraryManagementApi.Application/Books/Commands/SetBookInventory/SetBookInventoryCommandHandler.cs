using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Books.Commands.SetBookInventory;

public class SetBookInventoryCommandHandler(IApplicationDbContext context) : IRequestHandler<SetBookInventoryCommand, BookInventoryDto>
{
    public async Task<BookInventoryDto> Handle(SetBookInventoryCommand request, CancellationToken cancellationToken)
    {
        var bookExists = await context.Books.AnyAsync(b => b.Id == request.BookId, cancellationToken);
        if (!bookExists)
        {
            throw new NotFoundException(nameof(Book), request.BookId);
        }

        var branch = await context.Branches.SingleOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.BranchId);

        var inventory = await context.BookInventories
            .SingleOrDefaultAsync(i => i.BookId == request.BookId && i.BranchId == request.BranchId, cancellationToken);

        if (inventory is null)
        {
            inventory = BookInventory.Create(request.BookId, request.BranchId, request.TotalCopies);
            context.BookInventories.Add(inventory);
        }
        else
        {
            inventory.SetTotalCopies(request.TotalCopies);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new BookInventoryDto(branch.Id, branch.Name, inventory.TotalCopies, inventory.AvailableCopies);
    }
}
