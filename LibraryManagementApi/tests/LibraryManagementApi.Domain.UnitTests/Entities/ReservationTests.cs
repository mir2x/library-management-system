using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.UnitTests.Entities;

public class ReservationTests
{
    private static Reservation CreateReservation() => Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Create_DefaultsToPendingAndSetsReservedAtUtc()
    {
        var before = DateTime.UtcNow;
        var reservation = CreateReservation();
        var after = DateTime.UtcNow;

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.InRange(reservation.ReservedAtUtc, before, after);
        Assert.Null(reservation.ReadyAtUtc);
        Assert.Null(reservation.FulfilledAtUtc);
        Assert.Null(reservation.CancelledAtUtc);
    }

    [Fact]
    public void MarkReady_FromPending_SetsStatusAndReadyAtUtc()
    {
        var reservation = CreateReservation();

        reservation.MarkReady();

        Assert.Equal(ReservationStatus.Ready, reservation.Status);
        Assert.NotNull(reservation.ReadyAtUtc);
    }

    [Fact]
    public void MarkReady_WhenNotPending_ThrowsDomainException()
    {
        var reservation = CreateReservation();
        reservation.MarkReady();

        Assert.Throws<DomainException>(reservation.MarkReady);
    }

    [Fact]
    public void MarkFulfilled_FromReady_SetsStatusAndFulfilledAtUtc()
    {
        var reservation = CreateReservation();
        reservation.MarkReady();

        reservation.MarkFulfilled();

        Assert.Equal(ReservationStatus.Fulfilled, reservation.Status);
        Assert.NotNull(reservation.FulfilledAtUtc);
    }

    [Fact]
    public void MarkFulfilled_WhenStillPending_ThrowsDomainException()
    {
        var reservation = CreateReservation();

        Assert.Throws<DomainException>(reservation.MarkFulfilled);
    }

    [Fact]
    public void Cancel_FromPending_SetsStatusAndCancelledAtUtc()
    {
        var reservation = CreateReservation();

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.NotNull(reservation.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_FromReady_Succeeds()
    {
        var reservation = CreateReservation();
        reservation.MarkReady();

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyFulfilled_ThrowsDomainException()
    {
        var reservation = CreateReservation();
        reservation.MarkReady();
        reservation.MarkFulfilled();

        Assert.Throws<DomainException>(reservation.Cancel);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException()
    {
        var reservation = CreateReservation();
        reservation.Cancel();

        Assert.Throws<DomainException>(reservation.Cancel);
    }
}
