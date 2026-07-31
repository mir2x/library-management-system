using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Application.UnitTests.Reservations;

public class ReservationAllocatorTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly ReservationAllocator _allocator;

    public ReservationAllocatorTests()
    {
        _allocator = new ReservationAllocator(_context);
    }

    [Fact]
    public async Task ReleaseCopyAsync_WithNoPendingReservations_ReleasesToGeneralAvailability()
    {
        var bookId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var inventory = BookInventory.Create(bookId, branchId, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);
        await _context.SaveChangesAsync(CancellationToken.None);

        await _allocator.ReleaseCopyAsync(bookId, branchId, CancellationToken.None);

        Assert.Equal(1, inventory.AvailableCopies);
    }

    [Fact]
    public async Task ReleaseCopyAsync_WithPendingReservation_MarksItReadyWithoutTouchingInventory()
    {
        var bookId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var inventory = BookInventory.Create(bookId, branchId, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);

        var reservation = Reservation.Create(Guid.NewGuid(), bookId, branchId);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        await _allocator.ReleaseCopyAsync(bookId, branchId, CancellationToken.None);

        Assert.Equal(ReservationStatus.Ready, reservation.Status);
        Assert.Equal(0, inventory.AvailableCopies);
    }

    [Fact]
    public async Task ReleaseCopyAsync_WithMultiplePendingReservations_PicksOldestFirst()
    {
        var bookId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var inventory = BookInventory.Create(bookId, branchId, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);

        var older = Reservation.Create(Guid.NewGuid(), bookId, branchId);
        _context.Reservations.Add(older);
        await _context.SaveChangesAsync(CancellationToken.None);

        var newer = Reservation.Create(Guid.NewGuid(), bookId, branchId);
        _context.Reservations.Add(newer);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Force a clear ordering rather than relying on sub-millisecond creation timing.
        _context.Entry(older).Property(nameof(Reservation.ReservedAtUtc)).CurrentValue = DateTime.UtcNow.AddMinutes(-10);
        await _context.SaveChangesAsync(CancellationToken.None);

        await _allocator.ReleaseCopyAsync(bookId, branchId, CancellationToken.None);

        Assert.Equal(ReservationStatus.Ready, older.Status);
        Assert.Equal(ReservationStatus.Pending, newer.Status);
    }

    [Fact]
    public async Task ReleaseCopyAsync_IgnoresReservationsForDifferentBookOrBranch()
    {
        var bookId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var inventory = BookInventory.Create(bookId, branchId, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);

        var differentBookReservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), branchId);
        var differentBranchReservation = Reservation.Create(Guid.NewGuid(), bookId, Guid.NewGuid());
        _context.Reservations.AddRange(differentBookReservation, differentBranchReservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        await _allocator.ReleaseCopyAsync(bookId, branchId, CancellationToken.None);

        Assert.Equal(ReservationStatus.Pending, differentBookReservation.Status);
        Assert.Equal(ReservationStatus.Pending, differentBranchReservation.Status);
        Assert.Equal(1, inventory.AvailableCopies);
    }
}
