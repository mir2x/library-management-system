using FluentValidation;

namespace LibraryManagementApi.Application.Reservations.Commands.CreateMyReservation;

public class CreateMyReservationCommandValidator : AbstractValidator<CreateMyReservationCommand>
{
    public CreateMyReservationCommandValidator()
    {
        RuleFor(x => x.BookId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
    }
}
