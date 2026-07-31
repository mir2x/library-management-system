using FluentValidation;

namespace LibraryManagementApi.Application.Reports.Queries.GetOverdueLoansReport;

public class GetOverdueLoansReportQueryValidator : AbstractValidator<GetOverdueLoansReportQuery>
{
    public GetOverdueLoansReportQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
