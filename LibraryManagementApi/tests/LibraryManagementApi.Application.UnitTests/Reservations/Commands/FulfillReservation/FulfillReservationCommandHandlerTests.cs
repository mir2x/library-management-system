using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Application.Reservations.Commands.FulfillReservation;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Commands.FulfillReservation;

public class FulfillReservationCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly FulfillReservationCommandHandler _handler;

    public FulfillReservationCommandHandlerTests()
    {
        _handler = new FulfillReservationCommandHandler(_context, new LoanEligibilityChecker(_context));
    }

    private async Task<(Member Member, Book Book, Branch Branch, Reservation Reservation, BookInventory Inventory)> SeedReadyReservationAsync()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);

        var inventory = BookInventory.Create(book.Id, branch.Id, 1);
        inventory.Borrow(); // held for this reservation
        _context.BookInventories.Add(inventory);
        await _context.SaveChangesAsync(CancellationToken.None);

        var reservation = Reservation.Create(member.Id, book.Id, branch.Id);
        reservation.MarkReady();
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (member, book, branch, reservation, inventory);
    }

    [Fact]
    public async Task Handle_WithReadyReservation_CreatesLoanWithoutDecrementingInventoryAgain()
    {
        var (member, book, _, reservation, inventory) = await SeedReadyReservationAsync();

        var result = await _handler.Handle(new FulfillReservationCommand(reservation.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(member.FullName, result.Value!.MemberName);
        Assert.Equal(book.Title, result.Value!.BookTitle);
        Assert.Equal(ReservationStatus.Fulfilled, reservation.Status);
        Assert.Equal(0, inventory.AvailableCopies);
        Assert.Single(_context.Loans);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new FulfillReservationCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenStillPending_ThrowsDomainException()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var reservation = Reservation.Create(member.Id, book.Id, branch.Id);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new FulfillReservationCommand(reservation.Id);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSuspendedMember_ReturnsFailureWithoutFulfilling()
    {
        var (member, _, _, reservation, _) = await SeedReadyReservationAsync();
        member.Suspend();
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new FulfillReservationCommand(reservation.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ReservationStatus.Ready, reservation.Status);
    }
}
