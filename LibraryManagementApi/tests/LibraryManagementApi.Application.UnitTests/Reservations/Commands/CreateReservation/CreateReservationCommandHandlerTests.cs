using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Application.Reservations.Commands.CreateReservation;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly CreateReservationCommandHandler _handler;

    public CreateReservationCommandHandlerTests()
    {
        _handler = new CreateReservationCommandHandler(_context, new ReservationCreator(_context));
    }

    [Fact]
    public async Task Handle_WithUnknownMemberId_ThrowsNotFoundException()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateReservationCommand(Guid.NewGuid(), book.Id, branch.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithKnownMemberAndFullyCheckedOutBook_CreatesReservation()
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

        var command = new CreateReservationCommand(member.Id, book.Id, branch.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
    }
}
