using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reservations;

public class ReservationCreatorTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly ReservationCreator _creator;

    public ReservationCreatorTests()
    {
        _creator = new ReservationCreator(_context);
    }

    private async Task<(Member Member, Book Book, Branch Branch)> SeedFullyCheckedOutBookAsync()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);

        var inventory = BookInventory.Create(book.Id, branch.Id, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (member, book, branch);
    }

    [Fact]
    public async Task CreateAsync_WithFullyCheckedOutBook_CreatesPendingReservation()
    {
        var (member, book, branch) = await SeedFullyCheckedOutBookAsync();

        var result = await _creator.CreateAsync(member, book.Id, branch.Id, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value!.QueuePosition);
        Assert.Single(_context.Reservations);
    }

    [Fact]
    public async Task CreateAsync_WithSuspendedMember_ReturnsFailure()
    {
        var (member, book, branch) = await SeedFullyCheckedOutBookAsync();
        member.Suspend();
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _creator.CreateAsync(member, book.Id, branch.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Member is not active and cannot make reservations."], result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownBook_ThrowsNotFoundException()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _creator.CreateAsync(member, Guid.NewGuid(), branch.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithNoInventoryRecord_ReturnsFailure()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _creator.CreateAsync(member, book.Id, branch.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["This book is not stocked at this branch."], result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WithAvailableCopies_ReturnsFailure()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        _context.BookInventories.Add(BookInventory.Create(book.Id, branch.Id, 3));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _creator.CreateAsync(member, book.Id, branch.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["This book is currently available — borrow it directly instead of reserving."], result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WhenMemberAtMaxActiveReservations_ReturnsFailure()
    {
        var (member, book, branch) = await SeedFullyCheckedOutBookAsync();

        for (var i = 0; i < Reservation.MaxActiveReservationsPerMember; i++)
        {
            var otherBook = Book.Create($"Book {i}", "Author", $"978000000000{i}", "Genre", 2000, null);
            _context.Books.Add(otherBook);
            await _context.SaveChangesAsync(CancellationToken.None);
            var otherInventory = BookInventory.Create(otherBook.Id, branch.Id, 1);
            otherInventory.Borrow();
            _context.BookInventories.Add(otherInventory);
            _context.Reservations.Add(Reservation.Create(member.Id, otherBook.Id, branch.Id));
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _creator.CreateAsync(member, book.Id, branch.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(
            [$"Member has reached the maximum of {Reservation.MaxActiveReservationsPerMember} active reservations."],
            result.Errors);
    }

    [Fact]
    public async Task CreateAsync_WithExistingActiveReservationForSameBookAndBranch_ReturnsFailure()
    {
        var (member, book, branch) = await SeedFullyCheckedOutBookAsync();
        _context.Reservations.Add(Reservation.Create(member.Id, book.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _creator.CreateAsync(member, book.Id, branch.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Member already has an active reservation for this book at this branch."], result.Errors);
    }

    [Fact]
    public async Task CreateAsync_ComputesQueuePositionAmongExistingPendingReservations()
    {
        var (member, book, branch) = await SeedFullyCheckedOutBookAsync();
        var otherMember = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, null);
        _context.Members.Add(otherMember);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Reservations.Add(Reservation.Create(otherMember.Id, book.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _creator.CreateAsync(member, book.Id, branch.Id, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.QueuePosition);
    }
}
