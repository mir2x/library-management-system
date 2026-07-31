using FluentValidation;

namespace LibraryManagementApi.Application.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        // PATCH semantics: a null field means "leave unchanged", so rules only apply to
        // fields the caller actually included in the request.
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.Address is not null);

        RuleFor(x => x.ContactNumber)
            .MaximumLength(30);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email is not null);
    }
}
