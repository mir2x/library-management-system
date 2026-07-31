using FluentValidation;

namespace LibraryManagementApi.Application.Branches.Commands.CreateBranch;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ContactNumber)
            .MaximumLength(30);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email is not null);
    }
}
