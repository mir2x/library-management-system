using FluentValidation;

namespace LibraryManagementApi.Application.Loans.Commands.BorrowBook;

public class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEqual(Guid.Empty);
        RuleFor(x => x.BookId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
    }
}
