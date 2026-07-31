using FluentValidation;

namespace LibraryManagementApi.Application.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEqual(Guid.Empty);
        RuleFor(x => x.BookId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
    }
}
