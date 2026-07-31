using FluentValidation;

namespace LibraryManagementApi.Application.Members.Queries.GetMembers;

public class GetMembersQueryValidator : AbstractValidator<GetMembersQuery>
{
    public GetMembersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
