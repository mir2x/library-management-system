using LibraryManagementApi.Application.Reservations.Commands.CreateMyReservation;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Commands.CreateMyReservation;

public class CreateMyReservationCommandValidatorTests
{
    private readonly CreateMyReservationCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new CreateMyReservationCommand("user-1", Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyBookId_HasError()
    {
        var command = new CreateMyReservationCommand("user-1", Guid.Empty, Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMyReservationCommand.BookId));
    }
}
