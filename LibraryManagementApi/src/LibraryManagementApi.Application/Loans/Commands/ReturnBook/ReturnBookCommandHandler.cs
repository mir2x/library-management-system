using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Loans.Commands.ReturnBook;

public class ReturnBookCommandHandler(IApplicationDbContext context, ReservationAllocator reservationAllocator)
    : IRequestHandler<ReturnBookCommand, Result>
{
    public async Task<Result> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var loan = await context.Loans.SingleOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.Id);

        loan.MarkReturned();

        // Hands the copy to the next pending reservation for this book/branch if one exists,
        // otherwise releases it back to general availability.
        await reservationAllocator.ReleaseCopyAsync(loan.BookId, loan.BranchId, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
