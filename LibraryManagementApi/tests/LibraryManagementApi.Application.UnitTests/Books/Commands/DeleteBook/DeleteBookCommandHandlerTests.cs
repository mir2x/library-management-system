using LibraryManagementApi.Application.Books.Commands.DeleteBook;
using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Books.Commands.DeleteBook;

public class DeleteBookCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly DeleteBookCommandHandler _handler;

    public DeleteBookCommandHandlerTests()
    {
        _handler = new DeleteBookCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingBook_DeactivatesItAndReturnsSuccess()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new DeleteBookCommand(book.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(book.IsActive);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new DeleteBookCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
