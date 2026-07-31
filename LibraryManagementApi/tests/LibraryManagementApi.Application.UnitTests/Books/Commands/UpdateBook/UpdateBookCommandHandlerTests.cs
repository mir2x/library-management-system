using LibraryManagementApi.Application.Books.Commands.UpdateBook;
using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Books.Commands.UpdateBook;

public class UpdateBookCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly UpdateBookCommandHandler _handler;

    public UpdateBookCommandHandlerTests()
    {
        _handler = new UpdateBookCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithPartialFields_UpdatesOnlyProvidedFieldsAndLeavesOthersUnchanged()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, "Original description");
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateBookCommand(book.Id, "Clean Code (2nd Ed.)", null, null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Clean Code (2nd Ed.)", book.Title);
        Assert.Equal("Robert C. Martin", book.Author);
        Assert.Equal("Software Engineering", book.Genre);
        Assert.Equal(2008, book.PublishedYear);
        Assert.Equal("Original description", book.Description);
        Assert.Equal("9780132350884", book.Isbn);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), "New Title", null, null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
