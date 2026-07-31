using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Application.Reservations.Commands.CancelReservation;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Commands.CancelReservation;

public class CancelReservationCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly CancelReservationCommandHandler _handler;

    public CancelReservationCommandHandlerTests()
    {
        _handler = new CancelReservationCommandHandler(_context, new ReservationAllocator(_context));
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new CancelReservationCommand(Guid.NewGuid(), "user-1", CallerIsStaff: false);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AsOwningMember_Succeeds()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), userId: "user-1");
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var reservation = Reservation.Create(member.Id, Guid.NewGuid(), Guid.NewGuid());
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CancelReservationCommand(reservation.Id, "user-1", CallerIsStaff: false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public async Task Handle_AsDifferentMember_ThrowsForbiddenAccessException()
    {
        var owner = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), userId: "owner-user");
        var otherUser = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, Guid.NewGuid(), userId: "other-user");
        _context.Members.AddRange(owner, otherUser);
        await _context.SaveChangesAsync(CancellationToken.None);

        var reservation = Reservation.Create(owner.Id, Guid.NewGuid(), Guid.NewGuid());
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CancelReservationCommand(reservation.Id, "other-user", CallerIsStaff: false);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AsStaff_CanCancelAnyMembersReservation()
    {
        var owner = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), userId: "owner-user");
        _context.Members.Add(owner);
        await _context.SaveChangesAsync(CancellationToken.None);

        var reservation = Reservation.Create(owner.Id, Guid.NewGuid(), Guid.NewGuid());
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CancelReservationCommand(reservation.Id, "librarian-user", CallerIsStaff: true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public async Task Handle_WhenAlreadyFulfilled_ThrowsDomainException()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), userId: "user-1");
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var reservation = Reservation.Create(member.Id, Guid.NewGuid(), Guid.NewGuid());
        reservation.MarkReady();
        reservation.MarkFulfilled();
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CancelReservationCommand(reservation.Id, "user-1", CallerIsStaff: false);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CancellingAReadyReservation_ReleasesTheHeldCopy()
    {
        var branch = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var inventory = BookInventory.Create(bookId, branch, 1);
        inventory.Borrow(); // held for this reservation, not in general circulation
        _context.BookInventories.Add(inventory);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), userId: "user-1");
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var reservation = Reservation.Create(member.Id, bookId, branch);
        reservation.MarkReady();
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CancelReservationCommand(reservation.Id, "user-1", CallerIsStaff: false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, inventory.AvailableCopies);
    }
}
