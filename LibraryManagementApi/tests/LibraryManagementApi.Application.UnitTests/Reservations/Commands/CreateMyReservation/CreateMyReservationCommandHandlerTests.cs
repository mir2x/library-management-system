using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Application.Reservations.Commands.CreateMyReservation;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Commands.CreateMyReservation;

public class CreateMyReservationCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly CreateMyReservationCommandHandler _handler;

    public CreateMyReservationCommandHandlerTests()
    {
        _handler = new CreateMyReservationCommandHandler(_context, new ReservationCreator(_context));
    }

    [Fact]
    public async Task Handle_WithNoLinkedMember_ThrowsNotFoundException()
    {
        var command = new CreateMyReservationCommand("user-without-member", Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithLinkedMemberAndFullyCheckedOutBook_CreatesReservation()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, userId: "user-1");
        _context.Members.Add(member);
        var inventory = BookInventory.Create(book.Id, branch.Id, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateMyReservationCommand("user-1", book.Id, branch.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
    }
}
