using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Loans.Commands.BorrowBook;

public class BorrowBookCommandHandler(IApplicationDbContext context, LoanEligibilityChecker eligibilityChecker)
    : IRequestHandler<BorrowBookCommand, Result<LoanDto>>
{
    public async Task<Result<LoanDto>> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        var book = await context.Books.SingleOrDefaultAsync(b => b.Id == request.BookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), request.BookId);

        var branch = await context.Branches.SingleOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.BranchId);

        var inventory = await context.BookInventories
            .SingleOrDefaultAsync(i => i.BookId == request.BookId && i.BranchId == request.BranchId, cancellationToken);

        if (inventory is null || inventory.AvailableCopies <= 0)
        {
            return Result<LoanDto>.Failure(["No copies of this book are available at this branch."]);
        }

        var ineligibleReason = await eligibilityChecker.CheckAsync(member, request.BookId, cancellationToken);
        if (ineligibleReason is not null)
        {
            return Result<LoanDto>.Failure([ineligibleReason]);
        }

        // A race-condition safety net, not the primary check above: if two requests both pass
        // the AvailableCopies check concurrently, whichever commits second hits this guard.
        inventory.Borrow();

        var loan = Loan.Create(request.MemberId, request.BookId, request.BranchId);
        context.Loans.Add(loan);

        await context.SaveChangesAsync(cancellationToken);

        return Result<LoanDto>.Success(new LoanDto(
            loan.Id, loan.MemberId, member.FullName, loan.BookId, book.Title, loan.BranchId, branch.Name,
            loan.BorrowedAtUtc, loan.DueDateUtc, loan.ReturnedAtUtc, loan.Status, loan.IsOverdue));
    }
}
