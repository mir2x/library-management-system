using LibraryManagementApi.Application.Reservations.Queries.GetMyReservations;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryValidatorTests
{
    private readonly GetMyReservationsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaultPaging_HasNoErrors()
    {
        var query = new GetMyReservationsQuery("user-1");

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPageNumberLessThanOne_HasError()
    {
        var query = new GetMyReservationsQuery("user-1", PageNumber: 0);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetMyReservationsQuery.PageNumber));
    }
}
