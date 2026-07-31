using LibraryManagementApi.Application.Books.Commands.CreateBook;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Books.Commands.CreateBook;

public class CreateBookCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly CreateBookCommandHandler _handler;

    public CreateBookCommandHandlerTests()
    {
        _handler = new CreateBookCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithUniqueIsbn_CreatesBookAndReturnsDto()
    {
        var command = new CreateBookCommand("Clean Code", "Robert C. Martin", "978-0-13-235088-4", "Software Engineering", 2008, "Description");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Clean Code", result.Value!.Title);
        Assert.Equal("9780132350884", result.Value!.Isbn);
        Assert.True(result.Value!.IsActive);
    }

    [Fact]
    public async Task Handle_WithDuplicateActiveIsbn_ReturnsFailure()
    {
        _context.Books.Add(Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null));
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateBookCommand("Clean Code (Reprint)", "Robert C. Martin", "978-0-13-235088-4", "Software Engineering", 2008, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["A book with this ISBN already exists."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithSameIsbnAsDeactivatedBook_Succeeds()
    {
        var deactivated = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        deactivated.Deactivate();
        _context.Books.Add(deactivated);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateBookCommand("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
    }
}
