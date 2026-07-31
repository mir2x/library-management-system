using FluentValidation;

namespace LibraryManagementApi.Application.Reports.Queries.GetMostBorrowedBooksReport;

public class GetMostBorrowedBooksReportQueryValidator : AbstractValidator<GetMostBorrowedBooksReportQuery>
{
    public GetMostBorrowedBooksReportQueryValidator()
    {
        RuleFor(x => x.Top)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.ToUtc)
            .GreaterThanOrEqualTo(x => x.FromUtc)
            .When(x => x.FromUtc is not null && x.ToUtc is not null)
            .WithMessage("ToUtc must not be earlier than FromUtc.");
    }
}
