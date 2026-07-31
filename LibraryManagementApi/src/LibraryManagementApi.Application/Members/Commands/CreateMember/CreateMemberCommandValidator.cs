using FluentValidation;

namespace LibraryManagementApi.Application.Members.Commands.CreateMember;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .MaximumLength(30);

        RuleFor(x => x.Address)
            .MaximumLength(500);

        RuleFor(x => x.HomeBranchId)
            .NotEqual(Guid.Empty);
    }
}
