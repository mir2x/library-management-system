using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations.Commands.FulfillReservation;

public class FulfillReservationCommandHandler(IApplicationDbContext context, LoanEligibilityChecker eligibilityChecker)
    : IRequestHandler<FulfillReservationCommand, Result<LoanDto>>
{
    public async Task<Result<LoanDto>> Handle(FulfillReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations.SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == reservation.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), reservation.MemberId);

        var book = await context.Books.SingleOrDefaultAsync(b => b.Id == reservation.BookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), reservation.BookId);

        var branch = await context.Branches.SingleOrDefaultAsync(b => b.Id == reservation.BranchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), reservation.BranchId);

        var ineligibleReason = await eligibilityChecker.CheckAsync(member, reservation.BookId, cancellationToken);
        if (ineligibleReason is not null)
        {
            return Result<LoanDto>.Failure([ineligibleReason]);
        }

        reservation.MarkFulfilled();

        // The copy was already held out of general availability when the reservation became
        // Ready (see ReservationAllocator) — it was never returned to the pool, so borrowing it
        // now must not decrement AvailableCopies a second time.
        var loan = Loan.Create(reservation.MemberId, reservation.BookId, reservation.BranchId);
        context.Loans.Add(loan);

        await context.SaveChangesAsync(cancellationToken);

        return Result<LoanDto>.Success(new LoanDto(
            loan.Id, loan.MemberId, member.FullName, loan.BookId, book.Title, loan.BranchId, branch.Name,
            loan.BorrowedAtUtc, loan.DueDateUtc, loan.ReturnedAtUtc, loan.Status, loan.IsOverdue));
    }
}
