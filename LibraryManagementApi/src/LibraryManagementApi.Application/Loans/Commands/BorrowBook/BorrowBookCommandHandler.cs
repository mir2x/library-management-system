using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Loans.Commands.BorrowBook;

public class BorrowBookCommandHandler(IApplicationDbContext context) : IRequestHandler<BorrowBookCommand, Result<LoanDto>>
{
    public async Task<Result<LoanDto>> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var member = await context.Members.SingleOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        if (member.Status != MembershipStatus.Active)
        {
            return Result<LoanDto>.Failure(["Member is not active and cannot borrow books."]);
        }

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

        var activeLoanCount = await context.Loans
            .CountAsync(l => l.MemberId == request.MemberId && l.Status == LoanStatus.Active, cancellationToken);

        if (activeLoanCount >= Loan.MaxActiveLoansPerMember)
        {
            return Result<LoanDto>.Failure([$"Member has reached the maximum of {Loan.MaxActiveLoansPerMember} active loans."]);
        }

        var alreadyBorrowed = await context.Loans
            .AnyAsync(l => l.MemberId == request.MemberId && l.BookId == request.BookId && l.Status == LoanStatus.Active, cancellationToken);

        if (alreadyBorrowed)
        {
            return Result<LoanDto>.Failure(["Member already has an active loan for this book."]);
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
