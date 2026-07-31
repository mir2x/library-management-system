using LibraryManagementApi.Application.Books.Commands.SetBookInventory;
using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Books.Commands.SetBookInventory;

public class SetBookInventoryCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly SetBookInventoryCommandHandler _handler;

    public SetBookInventoryCommandHandlerTests()
    {
        _handler = new SetBookInventoryCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithNoExistingInventory_CreatesInventoryRecord()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Books.Add(book);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new SetBookInventoryCommand(book.Id, branch.Id, 5), CancellationToken.None);

        Assert.Equal(branch.Name, result.BranchName);
        Assert.Equal(5, result.TotalCopies);
        Assert.Equal(5, result.AvailableCopies);
        Assert.Single(_context.BookInventories);
    }

    [Fact]
    public async Task Handle_WithExistingInventory_UpdatesTotalCopies()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Books.Add(book);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.BookInventories.Add(BookInventory.Create(book.Id, branch.Id, 5));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new SetBookInventoryCommand(book.Id, branch.Id, 8), CancellationToken.None);

        Assert.Equal(8, result.TotalCopies);
        Assert.Equal(8, result.AvailableCopies);
        Assert.Single(_context.BookInventories);
    }

    [Fact]
    public async Task Handle_WithUnknownBookId_ThrowsNotFoundException()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new SetBookInventoryCommand(Guid.NewGuid(), branch.Id, 5);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownBranchId_ThrowsNotFoundException()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new SetBookInventoryCommand(book.Id, Guid.NewGuid(), 5);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
