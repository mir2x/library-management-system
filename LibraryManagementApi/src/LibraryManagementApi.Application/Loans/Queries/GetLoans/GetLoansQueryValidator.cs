using FluentValidation;

namespace LibraryManagementApi.Application.Loans.Queries.GetLoans;

public class GetLoansQueryValidator : AbstractValidator<GetLoansQuery>
{
    public GetLoansQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
