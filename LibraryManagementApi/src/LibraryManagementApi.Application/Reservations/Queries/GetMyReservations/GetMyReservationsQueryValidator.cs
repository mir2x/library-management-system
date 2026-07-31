using FluentValidation;

namespace LibraryManagementApi.Application.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryValidator : AbstractValidator<GetMyReservationsQuery>
{
    public GetMyReservationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
