using LibraryManagementApi.Application.Books.Queries.GetBookById;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Books.Queries.GetBookById;

public class GetBookByIdQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetBookByIdQueryHandler _handler;

    public GetBookByIdQueryHandlerTests()
    {
        _handler = new GetBookByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingBookAndInventory_ReturnsDetailWithPerBranchBreakdown()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Books.Add(book);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.BookInventories.Add(BookInventory.Create(book.Id, branch.Id, 5));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBookByIdQuery(book.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(book.Title, result!.Title);
        Assert.Single(result.Inventory);
        Assert.Equal(branch.Name, result.Inventory[0].BranchName);
        Assert.Equal(5, result.Inventory[0].TotalCopies);
        Assert.Equal(5, result.Inventory[0].AvailableCopies);
    }

    [Fact]
    public async Task Handle_WithNoInventory_ReturnsEmptyInventoryList()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBookByIdQuery(book.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result!.Inventory);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNull()
    {
        var result = await _handler.Handle(new GetBookByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
