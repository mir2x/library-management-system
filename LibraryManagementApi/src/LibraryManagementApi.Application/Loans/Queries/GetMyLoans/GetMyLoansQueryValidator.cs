using FluentValidation;

namespace LibraryManagementApi.Application.Loans.Queries.GetMyLoans;

public class GetMyLoansQueryValidator : AbstractValidator<GetMyLoansQuery>
{
    public GetMyLoansQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
