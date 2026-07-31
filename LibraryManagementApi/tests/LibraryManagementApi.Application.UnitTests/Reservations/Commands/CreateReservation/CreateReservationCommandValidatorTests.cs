using LibraryManagementApi.Application.Reservations.Commands.CreateReservation;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidatorTests
{
    private readonly CreateReservationCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new CreateReservationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyMemberId_HasError()
    {
        var command = new CreateReservationCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReservationCommand.MemberId));
    }
}
