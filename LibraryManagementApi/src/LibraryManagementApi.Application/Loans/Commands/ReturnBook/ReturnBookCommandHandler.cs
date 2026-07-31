using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Loans.Commands.ReturnBook;

public class ReturnBookCommandHandler(IApplicationDbContext context) : IRequestHandler<ReturnBookCommand, Result>
{
    public async Task<Result> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var loan = await context.Loans.SingleOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.Id);

        loan.MarkReturned();

        var inventory = await context.BookInventories
            .SingleOrDefaultAsync(i => i.BookId == loan.BookId && i.BranchId == loan.BranchId, cancellationToken)
            ?? throw new InvalidOperationException("Inventory record missing for a book that was previously borrowed.");

        inventory.Return();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
