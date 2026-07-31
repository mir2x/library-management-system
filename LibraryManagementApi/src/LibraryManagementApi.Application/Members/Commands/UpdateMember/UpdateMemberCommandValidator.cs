using FluentValidation;

namespace LibraryManagementApi.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.FullName is not null);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256)
            .When(x => x.Email is not null);

        RuleFor(x => x.Phone)
            .MaximumLength(30);

        RuleFor(x => x.Address)
            .MaximumLength(500);

        RuleFor(x => x.HomeBranchId)
            .NotEqual(Guid.Empty)
            .When(x => x.HomeBranchId is not null);
    }
}
