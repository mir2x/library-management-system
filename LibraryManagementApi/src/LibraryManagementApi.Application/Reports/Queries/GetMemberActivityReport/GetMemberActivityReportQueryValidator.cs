using FluentValidation;

namespace LibraryManagementApi.Application.Reports.Queries.GetMemberActivityReport;

public class GetMemberActivityReportQueryValidator : AbstractValidator<GetMemberActivityReportQuery>
{
    public GetMemberActivityReportQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
